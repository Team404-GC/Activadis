# Activadis ELO Rating System

This module implements a full ELO-based ranking system: rating math, match recording (individual and team), leaderboards with database persistence, custom rank titles, and the API that exposes it all. It's split into four layers, from pure math up to HTTP.

```
EloCalculationService   →  pure math, no dependencies
EloService               →  orchestrates matches, profiles, categories
LeaderboardService       →  cache-first strategy with DB persistence
EloController            →  HTTP endpoints
```

---

## 1. `EloCalculationService` — the math layer

A static, framework-free class (no EF Core / ASP.NET / Blazor references) so it can be unit tested in isolation. It holds all the ELO formulas.

**Key constants**
- `InitialRating = 800` — starting rating for new players
- `MinRatingFloor = 100` / `DefaultMaxRating = 2000` (or `2500` for league tier) — ratings are clamped between these
- K-factor scales with experience via `GetKFactor(matchCount)`:
  - `< 10` matches → K = 100 (volatile, new players)
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
6. Rebuilds and saves the category leaderboard snapshot to the database.
7. Rebuilds and saves the overall leaderboard snapshot to the database.
8. Returns a `MatchResultDto` with all per-participant results.

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
- `GetCategoryLeaderboardAsync` / `GetOverallLeaderboardAsync` use a cache-first strategy:
  1. Check in-memory cache first (returns immediately if valid)
  2. If cache miss, load most recent snapshot from database
  3. If database miss, rebuild from current `Rating` records
  - Rebuilds include rank title assignment based on category's `RankTitles` dictionary
  - After rebuild, saves snapshot to database for future cache misses

### Categories
- CRUD operations: `GetCategoriesAsync`, `CreateCategoryAsync`, `UpdateCategoryAsync`
- Each category now includes a `RankTitles` dictionary mapping ELO rating thresholds to custom rank titles
  - Example: `{ 800: "Bronze", 1200: "Silver", 1600: "Gold" }`
  - Titles are assigned in leaderboard display based on player's current rating
  - Supports both category-specific and overall leaderboard custom titles

---

## 3. `LeaderboardService` — ranking + caching with database persistence

Keeps an in-memory `Dictionary<Guid, LeaderboardCache>` guarded by a single `lock`, keyed by category ID (with `Guid.Empty` reserved for the "overall" leaderboard).

### Cache-first architecture
- **On read** (`GetCategoryLeaderboardAsync` / `GetOverallLeaderboardAsync`):
  1. Check cache; return if valid (< 5 minutes old by default)
  2. Cache miss → load latest snapshot from `ILeaderboardRepository`
  3. DB miss → rebuild from current ratings, assign rank titles, save to DB

- **On write** (after match recording):
  1. Invalidate in-memory cache for affected category
  2. Rebuild leaderboard from current ratings with updated `RankTitles`
  3. Save snapshot to database via `ILeaderboardRepository.SaveLeaderboardAsync`
  4. Automatically invalidates overall leaderboard too (since overall rankings change)

### Methods
- `GetCategoryLeaderboardAsync(categoryId, ratings, category, cacheDurationMinutes)` — returns ranked list with optional rank titles
- `GetOverallLeaderboardAsync(allRatings, overallRankTitles, cacheDurationMinutes)` — groups ratings by user, computes weighted overall rating
- `SaveCategoryLeaderboardAsync(categoryId, entries, category)` — persists leaderboard snapshot to DB
- `SaveOverallLeaderboardAsync(entries)` — persists overall leaderboard snapshot to DB
- `InvalidateCache(categoryId)` — clears category cache and overall cache
- `ClearAllCache` — nukes all cached leaderboards (e.g., for admin tooling)

### Database storage
- `Leaderboard` entity stores a snapshot of rankings at a point in time
  - Fields: `Id`, `CategoryId` (null for overall), `IsOverall`, `Entries` (JSON-serialized list of `LeaderboardEntry` objects), `CreatedAt`
  - Indexed by `(CategoryId, IsOverall, CreatedAt desc)` for fast retrieval of latest
- `LeaderboardEntry` objects contain: `UserId`, `FullName`, `JobTitle`, `Rating`, `MatchCount`, `PeakRating`, `Rank`, `RankTitle`
- `ILeaderboardRepository.GetLatestCategoryLeaderboardAsync(categoryId)` returns the most recent category snapshot
- `ILeaderboardRepository.GetLatestOverallLeaderboardAsync()` returns the most recent overall snapshot
- Optional cleanup via `CleanupOldLeaderboardsAsync(keepCount)` to prune old snapshots (default keeps only the latest)

---

## 4. Rank Titles — Custom Rating Tiers

Categories can define custom rank titles to display player achievement levels on the leaderboard.

### Structure
- Each category stores a `Dictionary<double, string>` mapping minimum ELO thresholds to rank titles
- Example: `{ 800: "Novice", 1100: "Intermediate", 1400: "Advanced", 1700: "Expert" }`

### Assignment logic
- For each player on the leaderboard, find the highest threshold their rating meets
- If no thresholds are defined, `RankTitle` is null (not displayed)
- Example: A player with 1250 ELO against the above map would receive "Intermediate"

### Frontend support
- Category management UI (`Categories.razor`) now includes a `RankTitlesComponent` for adding/editing titles
- Leaderboard display (`Leaderboard.razor`) shows rank titles in a dedicated column
- DTOs (`CategoryDto`, `LeaderboardEntryDto`) include `RankTitles` and `RankTitle` fields respectively

---

## 5. `EloController` — HTTP surface

A standard `[Authorize]`-protected ASP.NET Core controller at `/Elo`, thin wrapper over `IEloService` with uniform `ApiResponse<T>` envelopes and try/catch → 500 on unexpected errors.

| Endpoint | Method | Auth | Purpose |
|---|---|---|---|
| `/Elo/leaderboard/overall` | GET | any authorized user | Overall leaderboard with rank titles |
| `/Elo/leaderboard/category/{categoryId}` | GET | any authorized user | Category leaderboard with rank titles |
| `/Elo/player/{userId}` | GET | any authorized user | Player profile + match history |
| `/Elo/categories` | GET | any authorized user | List active categories with rank title definitions |
| `/Elo/match/record` | POST | **Admin** | Record a match, update ratings, persist leaderboards |
| `/Elo/categories` | POST | **Admin** | Create a category (optionally with rank titles) |
| `/Elo/categories/{categoryId}` | PUT | **Admin** | Update a category (name, description, rank titles, etc.) |

Match recording and category management are Admin-only; everything else is readable by any authenticated user.

---

## Data flow summary: Recording a match

```
POST /Elo/match/record
  → EloController.RecordMatch
    → EloService.RecordMatchAsync
      → ProcessIndividualMatchAsync or ProcessTeamMatchAsync
        → Update Rating entities (CurrentRating, MatchCount, PeakRating)
        → Persist MatchParticipant records
      → LeaderboardService.InvalidateCache(categoryId)
      → LeaderboardService.GetCategoryLeaderboardAsync (rebuild + assign rank titles)
      → LeaderboardService.SaveCategoryLeaderboardAsync (persist snapshot)
      → LeaderboardService.GetOverallLeaderboardAsync (rebuild + assign rank titles)
      → LeaderboardService.SaveOverallLeaderboardAsync (persist snapshot)
      → Return MatchResultDto
```

---

## Data flow summary: Reading a leaderboard (cache-first)

```
GET /Elo/leaderboard/category/{categoryId}
  → EloController.GetCategoryLeaderboard
    → EloService.GetCategoryLeaderboardAsync
      → LeaderboardService.GetCategoryLeaderboardAsync
        1. Check in-memory cache (hits if < 5 min old)
        2. [Cache miss] → ILeaderboardRepository.GetLatestCategoryLeaderboardAsync
        3. [DB miss] → Load all Rating records for category
              → Build leaderboard from ratings
              → Assign rank titles using category.RankTitles
              → Save to database
              → Cache result
        → Return LeaderboardEntry list with RankTitle fields
      → Map to LeaderboardEntryDto
  → Return API response
```

---

## Seeding

The `RatingSeeder` calculates realistic seed ratings by simulating all seeded matches sequentially using the K-factor logic. This ensures:
- New players (0-9 matches) use K=100 for high volatility
- Transitioning players (10-29 matches) use K=20 for gradual stabilization  
- Experienced players (30+ matches) use K=10 for steady-state
- Peak ratings reflect the highest rating achieved after the match sequence

Match seeding is defined in `MatchSeeder.cs` and applied in order to generate realistic rating progressions.
        → EloCalculationService (pure math: expected score, new rating)
        → RatingRepository / MatchParticipantRepository (persist)
      → LeaderboardService.InvalidateCache
  ← MatchResultDto (per-participant old/new rating, rating change)
```

The separation keeps `EloCalculationService` trivially testable (no mocking needed), while `EloService` owns all the "what data do I need and where does it get saved" logic, and `LeaderboardService` isolates the caching concern from both.


last: subsidie vanger