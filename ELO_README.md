# Activadis ELO Rating System

This module implements a full ELO-based ranking system: rating math, match recording (individual and team), leaderboards, and the API that exposes it all. It's split into four layers, from pure math up to HTTP.

```
EloCalculationService   →  pure math, no dependencies
EloService               →  orchestrates matches, profiles, categories
LeaderboardService        →  caches and builds ranked lists
EloController              →  HTTP endpoints
```

---

## 1. `EloCalculationService` — the math layer

A static, framework-free class (no EF Core / ASP.NET / Blazor references) so it can be unit tested in isolation. It holds all the ELO formulas.

**Key constants**
- `InitialRating = 1200` — starting rating for new players
- `MinRatingFloor = 100` / `DefaultMaxRating = 2000` (or `2500` for league tier) — ratings are clamped between these
- K-factor scales with experience via `GetKFactor(matchCount)`:
  - `< 10` matches → K = 40 (volatile, new players)
  - `< 30` matches → K = 20
  - otherwise → K = 10 (stable, experienced players)

**Core formulas**
- `CalculateExpectedScore(playerRating, opponentRating)` — standard ELO expected-score formula: `1 / (1 + 10^((opponent - player)/400))`
- `CalculateNewRating(current, actual, expected, matchCount, maxRating)` — `NewRating = Current + K * (Actual - Expected)`, then clamped to the floor/ceiling
- `ConvertPlacementToScore(placement, totalParticipants, isTie)` — turns a finishing position into an actual score:
  - Ties → `0.5`
  - 1v1 → winner `1.0`, loser `0.0`
  - Group matches (3+) → linear interpolation from `1.0` (1st place) down to `0.0` (last place)
- `CalculateOverallRating(categoryRatingsWithCounts)` — a player's overall rating is the match-count-weighted average across all their category ratings

**Team support**
- `CalculateTeamAverageRating` — averages a team's member ratings into one number representing team strength
- `CalculateTeamExpectedScore` — runs the standard expected-score formula on the two teams' averages
- `CalculateTeamNewRatings` — every team member shares the same actual/expected score (the team's outcome), but each still gets their *own* K-factor based on their individual match count, so new and veteran teammates move by different amounts after the same result

**Validation helpers**: `IsValidActualScore` (0–1) and `IsValidRating` (within floor/ceiling).

---

## 2. `EloService` — orchestration

Implements `IEloService` and coordinates repositories + the calculation service to actually record matches and answer queries.

### Recording a match — `RecordMatchAsync`
1. Looks up the `Category`.
2. Creates a `Match` row (unfinalized).
3. Branches on `request.MatchType`:
   - `"team"` → `ProcessTeamMatchAsync`
   - anything else → `ProcessIndividualMatchAsync`
4. Marks the match finalized and saves it.
5. Invalidates the leaderboard cache for that category (see `LeaderboardService` below).
6. Returns a `MatchResultDto` with all per-participant results.

**Individual matches** (`ProcessIndividualMatchAsync`)
- For every participant: fetch/create their `Rating` for the category, fetch their `User`.
- Convert their placement to an actual score via `ConvertPlacementToScore`.
- Expected score is computed against *every other participant* and averaged (`CalculateIndividualExpectedScore`) — this is how the system handles matches with more than two people, not just 1v1.
- Apply `CalculateNewRating`, update `CurrentRating`, bump `MatchCount`, update `PeakRating` if it's a new high.
- Persist a `MatchParticipant` row recording placement, actual/expected score, and rating change.

**Team matches** (`ProcessTeamMatchAsync`)
- Groups participants by `TeamId` (ungrouped participants each become their own singleton "team" via a random GUID).
- Requires at least 2 teams; only the top two placed teams (by their best member placement) actually get rated — this is written as a two-team match, not free-for-all teams.
- Team strength = average of member ratings; expected scores come from `CalculateTeamExpectedScore`; actual score is `1.0`/`0.0` for win/loss or `0.5`/`0.5` on a tied placement.
- Every member of a team gets the same actual/expected score but their own K-factor-driven rating change (via `CalculateTeamNewRatings`), then a `MatchParticipant` row per member.

### Player profile — `GetPlayerProfileAsync`
- Pulls all of a user's category `Rating`s and converts them into `RatingDto`s.
- Computes an overall rating via the weighted-average formula.
- Pulls up to 20 recent `MatchParticipant` records and reconstructs before/after ratings for a match history.

### Leaderboards
- `GetCategoryLeaderboardAsync` / `GetOverallLeaderboardAsync` fetch ratings from the repository and hand them to `LeaderboardService`, which handles ranking and caching.

### Categories
- Straightforward CRUD: `GetCategoriesAsync`, `CreateCategoryAsync`, `UpdateCategoryAsync` (partial-update style — only overwrites fields present in the request).

---

## 3. `LeaderboardService` — ranking + caching

Keeps an in-memory `Dictionary<Guid, LeaderboardCache>` guarded by a single `lock`, keyed by category ID (with `Guid.Empty` reserved for the "overall" leaderboard).

- `GetCategoryLeaderboard` — returns the cached list if it's younger than `cacheDurationMinutes` (default 5), otherwise rebuilds it by sorting ratings descending and assigning ranks 1..N.
- `GetOverallLeaderboard` — groups all ratings by user, computes each user's overall rating with `EloCalculationService.CalculateOverallRating`, sorts descending, and ranks. Peak rating shown is the max across their categories.
- `InvalidateCache(categoryId)` — clears that category's cache *and* the overall cache (since any match affects the overall ranking too). Called automatically after every recorded match.
- `ClearAllCache` — nukes everything (e.g., for admin tooling or tests).

---

## 4. `EloController` — HTTP surface

A standard `[Authorize]`-protected ASP.NET Core controller at `/Elo`, thin wrapper over `IEloService` with uniform `ApiResponse<T>` envelopes and try/catch → 500 on unexpected errors.

| Endpoint | Method | Auth | Purpose |
|---|---|---|---|
| `/Elo/leaderboard/overall` | GET | any authorized user | Overall leaderboard |
| `/Elo/leaderboard/category/{categoryId}` | GET | any authorized user | Category leaderboard |
| `/Elo/player/{userId}` | GET | any authorized user | Player profile + match history |
| `/Elo/categories` | GET | any authorized user | List active categories |
| `/Elo/match/record` | POST | **Admin** | Record a match and update ratings |
| `/Elo/categories` | POST | **Admin** | Create a category |
| `/Elo/categories/{categoryId}` | PUT | **Admin** | Update a category |

Match recording and category management are Admin-only; everything else is readable by any authenticated user.

---

## Data flow summary

```
POST /Elo/match/record
  → EloController.RecordMatch
    → EloService.RecordMatchAsync
      → ProcessIndividualMatchAsync or ProcessTeamMatchAsync
        → EloCalculationService (pure math: expected score, new rating)
        → RatingRepository / MatchParticipantRepository (persist)
      → LeaderboardService.InvalidateCache
  ← MatchResultDto (per-participant old/new rating, rating change)
```

The separation keeps `EloCalculationService` trivially testable (no mocking needed), while `EloService` owns all the "what data do I need and where does it get saved" logic, and `LeaderboardService` isolates the caching concern from both.
