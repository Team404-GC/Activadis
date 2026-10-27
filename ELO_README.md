# Activadis ELO Rating System

This module implements a full ELO-based ranking system: rating math, match recording (individual and team), leaderboards with database persistence, custom rank titles, statistics for charts, and the API that exposes it all. It's split into four layers, from pure math up to HTTP.

```
EloCalculationService   →  pure math, no dependencies
EloService               →  orchestrates matches, profiles, categories, statistics
LeaderboardService       →  cache-first strategy with DB persistence
EloController            →  HTTP endpoints
```

---

## 1. `EloCalculationService` — the math layer

A static, framework-free class (no EF Core / ASP.NET / Blazor references) so it can be unit tested in isolation. It holds all the ELO formulas.

**Key constants**
- `InitialRating = 800` — starting rating for new players
- `MinRatingFloor = 100` / `DefaultMaxRating = 2000` (or `LeagueTierMaxRating = 2500` for league tier) — ratings are clamped between these
- K-factor scales with experience via `GetKFactor(matchCount)`:
  - `< 10` matches → K = 100 (volatile, new players)
  - `< 30` matches → K = 60
  - `< 60` matches → K = 45
  - otherwise → K = 35 (stable, experienced players)

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
1. Looks up the `Category`. When the request has an `ActivityId`, it also loads that activity and checks (in `ActivityCategoriesValidation.ValidateCategory`) that the category is one of the activity's ELO categories.
2. Creates a `Match` row (unfinalized), linked to the activity when an `ActivityId` was given.
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
- Pulls up to 20 recent `MatchParticipant` records and reconstructs before/after ratings for a match history. Each history row also carries the `ActivityId` and `ActivityName` of the activity the match was played at, when there is one (soft-deleted activities are left out).
- Also calculates, over all of the player's participations:
  - `WinRate` — share of participations with placement 1
  - `TotalActualScore` / `TotalExpectedScore` — sums of the stored scores
  - `Overperformance` — total actual score minus total expected score (positive = beats expectations)

### Leaderboards
- `GetCategoryLeaderboardAsync` / `GetOverallLeaderboardAsync` use a cache-first strategy:
  1. Check in-memory cache first (returns immediately if valid)
  2. If cache miss, load most recent snapshot from database
  3. If database miss, rebuild from current `Rating` records
  - Rebuilds include rank title assignment based on category's `RankTitles` dictionary
  - After rebuild, saves snapshot to database for future cache misses

### Categories
- CRUD operations: `GetCategoriesAsync`, `CreateCategoryAsync`, `UpdateCategoryAsync`
- Each category includes a `RankTitles` dictionary mapping ELO rating thresholds to custom rank titles
  - Example: `{ 800: "Bronze", 1200: "Silver", 1600: "Gold" }`
  - Titles are assigned in leaderboard display based on player's current rating
  - Supports both category-specific and overall leaderboard custom titles

### Statistics — `GetRatingHistoryAsync`, `GetSummaryAsync`, `GetActivityAsync`, `GetRecentMatchesAsync`
Read-only queries that feed the charts on the statistics page. All of them go through one repository method, `IMatchParticipantRepository.GetParticipationsAsync(categoryId?)`, which loads participations with their user, match and category, skipping soft-deleted rows. `EloService` then keeps only participations of **finalized** matches and does the grouping in memory.

- **`GetRatingHistoryAsync(categoryId, top, userIds)`** — rating over time per player
  - Picks the top N players of the category by current rating (`top` is clamped to 1–20), or only the players in `userIds` when given
  - Builds each line **backwards** from the player's `CurrentRating` by subtracting each `RatingChange` (newest match first), then reverses it
  - This way the line always ends on the real current rating. Because `RatingChange` is stored as the actual difference (`new - old`, after clamping to the floor/ceiling), the reconstruction stays exact even where a rating was clamped
- **`GetSummaryAsync(categoryId?)`** — total matches, matches this month, and number of active players (distinct users with at least one participation)
- **`GetActivityAsync(categoryId?, months)`** — matches per month for the last N months (`months` is clamped to 1–60). Months without matches are included with a count of 0, so a chart has no gaps
- **`GetRecentMatchesAsync(categoryId?, take)`** — latest matches first (`take` is clamped to 1–100), each with its category, date, notes and participants ordered by placement with their rating change. Also usable for "biggest gains and losses"

Some visuals need no extra endpoint and are calculated in the UI from the leaderboard data: rating distribution (100-point buckets), scatter plot of match count against rating, rank title spread, and the KPI values for average rating and highest peak rating.

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
  2. Rebuild leaderboard from current ratings with updated `RankTitles` (`forceRebuild: true`)
  3. Save snapshot to database via `ILeaderboardRepository.SaveLeaderboardAsync`
  4. Automatically invalidates overall leaderboard too (since overall rankings change)

### Methods
- `GetCategoryLeaderboardAsync(categoryId, ratings, category, cacheDurationMinutes, forceRebuild)` — returns ranked list with optional rank titles
- `GetOverallLeaderboardAsync(allRatings, overallRankTitles, cacheDurationMinutes, forceRebuild)` — groups ratings by user, computes weighted overall rating
- `SaveCategoryLeaderboardAsync(categoryId, entries, category)` — persists leaderboard snapshot to DB
- `SaveOverallLeaderboardAsync(entries)` — persists overall leaderboard snapshot to DB
- `InvalidateCache(categoryId)` — clears category cache and overall cache
- `ClearAllCache` — nukes all cached leaderboards (e.g., for admin tooling)

### Overall leaderboard titles
- When no titles are passed in, the overall leaderboard uses a built-in default tier map (`Nieuweling` from 0, `Groentje` 800, `Strijder` 1200, `Kampioen` 1500, `Opperbaas` 1800, `Legende` 2000)
- After ranking, the player in first place gets the special title `Ultime Covadiaan`, and the last-placed player (when there is more than one) gets `Subsidie Pakker`. These two override the tier title

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
- Category management UI (`PageCategories.razor`) includes a `RankTitlesComponent` for adding/editing titles
- When creating a new category the form is pre-filled with default titles: 0 "Beginner", 550 "Amateur", 750 "Gevorderde", 950 "Expert", 1100 "Meester" (they can still be removed or changed before saving)
- Leaderboard display (`PageLeaderboard.razor`) shows rank titles in a dedicated column
- DTOs (`CategoryDto`, `LeaderboardEntryDto`) include `RankTitles` and `RankTitle` fields respectively

---

## 5. `EloController` — HTTP surface

A standard `[Authorize]`-protected ASP.NET Core controller at `/Elo`, thin wrapper over `IEloService` with uniform `ApiResponse<T>` envelopes. `RecordMatch` returns 400 with the exception message on an `InvalidOperationException`; missing players and categories return 404.

| Endpoint | Method | Auth | Purpose |
|---|---|---|---|
| `/Elo/leaderboard/overall` | GET | any authorized user | Overall leaderboard with rank titles |
| `/Elo/leaderboard/category/{categoryId}` | GET | any authorized user | Category leaderboard with rank titles |
| `/Elo/player/{userId}` | GET | any authorized user | Player profile + match history + win rate and overperformance |
| `/Elo/categories` | GET | any authorized user | List active categories with rank title definitions |
| `/Elo/stats/rating-history` | GET | any authorized user | Rating over time per player (`categoryId`, `top`, optional `userIds`) |
| `/Elo/stats/summary` | GET | any authorized user | Total matches, matches this month, active players (optional `categoryId`) |
| `/Elo/stats/activity` | GET | any authorized user | Matches per month (optional `categoryId`, `months`) |
| `/Elo/matches/recent` | GET | any authorized user | Latest matches with participants and rating changes (optional `categoryId`, `take`) |
| `/Elo/match/record` | POST | **Admin** | Record a match, update ratings, persist leaderboards (optional `activityId` links it to an activity) |
| `/Elo/categories` | POST | **Admin** | Create a category (optionally with rank titles) |
| `/Elo/categories/{categoryId}` | PUT | **Admin** | Update a category (name, description, rank titles, etc.) |

Match recording and category management are Admin-only; everything else is readable by any authenticated user.

### Statistics DTOs (`Activadis.Shared.DTOs.Elo.Stats`)
- `PlayerRatingHistoryDto` — `UserId`, `FullName`, `Points` (list of `RatingHistoryPointDto` with `Date` and `Rating`)
- `EloSummaryDto` — `TotalMatches`, `MatchesThisMonth`, `ActivePlayers`
- `ActivityPointDto` — `Year`, `Month`, `MatchCount`
- `RecentMatchDto` — match id, category, date, notes and a list of `RecentMatchParticipantDto` (`UserId`, `FullName`, `Placement`, `RatingChange`)
- `PlayerProfileDto` gained `WinRate`, `TotalActualScore`, `TotalExpectedScore` and `Overperformance`

### UI service
`Activadis.UI.Application.Services.EloService` wraps every endpoint above through `IHttpService`. The statistics methods (`GetRatingHistoryAsync`, `GetSummaryAsync`, `GetActivityAsync`, `GetRecentMatchesAsync`) build the query string and leave out optional parameters that are not set.

---

## 6. Activities and the ELO ranking

An activity can be linked to one or more ELO categories (for example a bowling evening counts for "Bowlen"), and matches can be attached to the activity they were played at.

### Data model
- Many-to-many between `Activity` and `Category` (`Activity.Categories` / `Category.Activities`), stored in the join table `ActivityCategories`.
- `Match.ActivityId` is a nullable foreign key to `Activities`, so a match belongs to at most one activity. The foreign key uses `ON DELETE SET NULL`, and activities are soft-deleted anyway: cancelling or deleting an activity never deletes its matches or changes ratings.
- Both are added in the migration `AddedActivityCategoriesAndMatchActivity`.

### Rules (validation classes, not entities)
- `ActivityCategoriesValidation`
  - `ValidateCategories` — every chosen category must exist, and only active categories can be chosen (a category that is already linked may stay linked after it was deactivated).
  - `ValidateCategoryRemoval` — a category cannot be removed from an activity while a match in that category is still attached.
  - `ValidateCategory` — a match's category must be one of the activity's categories.
- `LinkMatchValidation`
  - `ValidateLink` — the match is not attached to this or another activity yet, and its category belongs to the activity.
  - `ValidateUnlink` — the match is attached to this activity.

An activity without categories can still be saved.

### Endpoints (`ActivityController`)
| Endpoint | Method | Auth | Purpose |
|---|---|---|---|
| `/Activity` | POST | **Admin** | Create an activity; `CategoryIds` (form field, repeated) picks its ELO categories |
| `/Activity/{id}` | GET | anyone | Activity detail; for logged-in users it includes `Categories` and the attached `Matches` (date, category, participants with placement and rating change) |
| `/Activity/{id}/categories` | PUT | **Admin** | Replace the activity's ELO categories |
| `/Activity/{id}/participants` | GET | **Admin** | Signed-up employees (external sign-ups have no account and are left out), used to pick players |
| `/Activity/{id}/matches/linkable` | GET | **Admin** | Finalized matches in one of the activity's categories, played from the start day of the activity until two days after it ended, that are not attached to any activity |
| `/Activity/{id}/matches` | POST | **Admin** | Attach an existing match (`MatchId`) |
| `/Activity/{id}/matches/{matchId}` | DELETE | **Admin** | Detach a match; the match and its ratings stay unchanged |

Non-admins get a 403 on every admin endpoint, also when calling it directly.

### UI
- `PageCreateActivity` — checkboxes for the active ELO categories.
- `PageActivityDetail` — shows the linked categories as pills (each links to its leaderboard) and the attached matches as cards (date, category, players with placement and rating change; the winner gets a gold badge). Administrators can change the categories, attach and detach matches, and use **Wedstrijd aanmaken**.
- **Wedstrijd aanmaken** opens `PageRecordMatch` at `/wedstrijd/nieuw/{id}`: the date is taken from the activity's start date, the categories are limited to the activity's categories (the administrator still picks one), players are chosen from a list of the activity's participants, and the saved match is linked to the activity.
- `PageRecordMatch` picks players with `UserSelectComponent`: a search box (name or e-mail) above a select. Without an activity the list holds every user, loaded from `GET /User` (Admin only, returns `UserResponse`: id, name, e-mail); with an activity it holds only the activity's participants.
- `PagePlayer` — the match history shows the activity a match was played at, linking to the activity.


---

## 7. ELO pages and styling

The ELO pages live in `Activadis.UI/Pages/Elo` and follow the `Page*.razor` naming of the rest of the UI:

| Page | Route | Notes |
|---|---|---|
| `PageLeaderboard` | `/ranglijst` | Category filter as pills ("Algemeen" plus one per category, the active one highlighted). Table with plaats, speler (with job title), rangtitel and rating; the top 3 get a gold, silver and bronze position badge. |
| `PagePlayer` | `/speler/{id}` | Stat tiles for the overall numbers, one card per category with rating, rank title, peak rating and match count, and the latest matches. |
| `PageCategories` | `/categorieen` | Category table with status pill; `CategoryFormComponent` and `RankTitlesComponent` open as a form card above it. |
| `PageRecordMatch` | `/wedstrijd/nieuw` | Form cards for the match data and participants (`UserSelectComponent` per player), result table after saving. |
| `PageStatistics` | `/statistieken` | KPI tiles and the charts/tables in cards. |

- Each page and component has its own scoped `.razor.css` with a short class prefix (`lb-`, `pl-`, `ca-`, `cf-`, `rt-`, `rm-`, `st-`, `ib-`, `us-`). There are no inline styles.
- Colours come from the variables in `wwwroot/css/app.css`. The ELO pages added `--AccentColorSoft` (also used by the activity pages, where it was missing), `--SuccessBg`, `--SuccessText`, `--GoldColor`, `--SilverColor` and `--BronzeColor`.
- Cards, buttons, form fields, back links and the loading/empty/error states (`*-state`) use the same look as the activity pages. Positive rating changes are green, negative ones red.
- Small screens (max 700px): less important table columns are hidden (`*-hide-small`), form grids go to one column and the participant rows of the match form stack, so no page scrolls horizontally.
- Ratings and rating changes are always shown with two decimals (for example `803.14` and `+12.50`) through `ToRating()` and `ToRatingChange()` in `Activadis.UI/Application/Extensions/EloExtensions.cs`. Chart values are rounded to two decimals as well. The charts on the statistics page have no toolbar (zoom, pan, reset, download) and zooming is turned off, via `CreateOptions<T>()` in `PageStatistics`.
- The "Elo ranking" dropdown in the navbar links to Categorieën, Ranglijst, Statistieken, Wedstrijd registreren (administrators only) and the personal profile.
- `InfoBubbleComponent` shows an info icon; clicking it opens the text in a small popover.
- `RatingDto` has a `RankTitle`, filled in `EloService.GetPlayerProfileAsync` with `LeaderboardService.GetRankTitle` (same rules as the leaderboard), so the player profile shows the rank title per category.

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

## Data flow summary: Reading statistics

```
GET /Elo/stats/rating-history | summary | activity   and   GET /Elo/matches/recent
  → EloController
    → EloService.GetRatingHistoryAsync / GetSummaryAsync / GetActivityAsync / GetRecentMatchesAsync
      → IMatchParticipantRepository.GetParticipationsAsync(categoryId?)
        (skips soft-deleted rows, includes User, Match and Category)
      → Keep only participations of finalized matches
      → Group and calculate in memory
        - rating history: start at CurrentRating, subtract RatingChange per match backwards
        - activity: distinct matches per month, empty months filled with 0
      → Map to Stats DTOs
  → Return API response
```

---

## Seeding

The `RatingSeeder` calculates realistic seed ratings by simulating all seeded matches sequentially using the K-factor logic. This ensures:
- New players (0-9 matches) use K=100 for high volatility
- Transitioning players (10-29 matches) use K=60
- Established players (30-59 matches) use K=45
- Experienced players (60+ matches) use K=35 for steady-state
- Peak ratings reflect the highest rating achieved after the match sequence

Match seeding is defined in `MatchSeeder.cs` and applied in order to generate realistic rating progressions.

Activities and ELO categories are connected in the seeders:
- `ActivitySeeder` adds tournament activities (Kantoorkampioenschap, Zomertoernooi, Darttoernooi, Spelletjesmiddag, Tafeltennis- en dartsmiddag, Darts op de vrijdagmiddagborrel, Tafeltennis- en tafelvoetbaltoernooi). Their dates are counted from today, so past ones stay past and upcoming ones stay upcoming. They use the seeder images in `Seeders/Images` (gaming, darts and tafeltennis).
- `CategorySeeder` links each category to the activities where it is played (Mario Kart also to the LAN-party and Game-avond). Activities that are not in the database are skipped.
- `MatchSeeder` links a match to the activity that was held on the match day and has the match's category. Matches on other days stay unlinked, so most matches are not connected to an activity.
- `MatchSeeder` also adds four matches of today (tafeltennis and darts) that stay unlinked, so linking a match to the activity "Tafeltennis- en dartsmiddag" (held today) can be tested by hand. `RatingSeeder` includes their rating changes.

## Unit tests

`Activadis.UnitTests` covers the Application layer: every service (activities, auth, users, sign-ups, waitlist, tokens, email, the four confirmation services, ELO and leaderboards), the validation classes and the extension methods. Repositories and other dependencies are mocked with Moq, shared test objects are built in `TestData.cs`. Run them with `dotnet test`.

The separation keeps `EloCalculationService` trivially testable (no mocking needed), while `EloService` owns all the "what data do I need and where does it get saved" logic, and `LeaderboardService` isolates the caching concern from both.