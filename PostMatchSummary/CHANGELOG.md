# 🔄 CHANGELOG - May 2026 Updates

All major changes and fixes implemented during this session.

---

## ✅ COMPLETED TASKS

### 1. **Add Match** - Translation & Fixes
- ✅ Translated title from "Kreiraj Novu Utakmicu" to "Create New Match"
- ✅ Translated save button from "💾 Sačuvaj Utakmicu" to "💾 Save Match"
- ✅ Fixed form route from `/api/postmatch/create` to `/postmatch/create`
- ✅ Translated all labels to English

### 2. **Add Player** - Dropdowns & Features
- ✅ Fixed Champion dropdown (was empty) - Added auto-seeding of 1700+ Riot champions from DDragon API
- ✅ Added "Game Mode" dropdown with options: Classic, ARAM, Rotating Game Mode, Teamfight Tactics
- ✅ All validation working

### 3. **Search Player** - UI Improvements
- ✅ Removed Blade template comments (`{{-- PROFILE HEADER --}}`, `{{-- MATCHES --}}`)
- ✅ Made entire match card clickable (not just button)
- ✅ Made player pills (names) clickable -> Opens SearchRiot result for that player
- ✅ Added Enter key support for form submission
- ✅ Changed back link route from `/api/players/search-riot` to `/players/search-riot`

### 4. **All Champions** - Complete Redesign
- ✅ Translated page title from "Championsi" to "All Champions"
- ✅ Translated all labels and buttons to English
- ✅ Champions already sorted alphabetically (verified in controller)
- ✅ Added new "Champions from Games" page (`/api/champions/from-games`)
- ✅ New page shows champions used in matches with appearance count

### 5. **All Players** - Improved Navigation
- ✅ Player names now link to SearchRiot result (not Details page)
- ✅ Only if player has RiotId, otherwise falls back to Details
- ✅ Prevents double-counting of player history

### 6. **Dashboard** - Data Accuracy
- ✅ Fixed unique player count - now groups by RiotId instead of SummonerName
- ✅ Shows full match ID (removed substring limitation)

### 7. **Sidebar Navigation** - Reorganization
- ✅ Moved "Search Player" to Overview section (main feature position)
- ✅ Moved "About" to bottom of navigation
- ✅ Added "Champions from Games" link in Champions section
- ✅ New Navigation Structure:
  1. Overview: Dashboard, Search Player
  2. Matches: All Matches, Add Match
  3. Champions: All Champions, Champions from Games, Add Champion
  4. Players: All Players, Add Player
  5. Teams: All Teams, Add Team
  6. About (bottom)

### 8. **Search Behavior** - Manual vs Auto
- ✅ Changed Champion Index search from auto (keyup) to manual (button click)
- ✅ Added explicit "Search" button
- ✅ "Clear" button still available

### 9. **Database** - Startup Initialization
- ✅ Added auto-seeding of all Riot champions from DDragon API
- ✅ Added IDENTITY reset to 0 for all tables (Champions, Players, Matches, Teams)
- ✅ Database is clean on every startup

### 10. **Code Quality** - Documentation
- ✅ Deleted outdated LAB documentation:
  - ❌ EF-ROUTING-PRINCIPLES.md
  - ❌ Lab4_IMPLEMENTACIJA.md
  - ❌ LAB4_VERIFIKACIJA.md
- ✅ Kept semantic-model.md (still relevant)
- ✅ Updated sitemap.md with current routing information

---

## 🔧 TECHNICAL IMPROVEMENTS

### Backend Changes
1. **RiotService.cs** - Added `FetchAllChampionsAsync()` method
   - Fetches champions from Riot DDragon API
   - Returns alphabetically sorted list

2. **Program.cs** - Enhanced database initialization
   - Clear all tables on startup
   - Reset IDENTITY seed to 0
   - Auto-seed champions before application runs

3. **ChampionController.cs** - Added `FromGames()` action
   - New route: `/api/champions/from-games`
   - Shows only champions used in matches
   - Displays appearance count

4. **MatchCacheService.cs** - Fixed unique player counting
   - Changed GroupBy from `SummonerName` to `RiotId`
   - Prevents double-counting of same player with different summoner names

### Frontend Changes (Views)
1. **PostMatch/Create.cshtml** - English translation + route fix
2. **Player/Create.cshtml** - Added Game Mode dropdown
3. **Player/SearchRiot.cshtml** - Added Enter key support
4. **Player/SearchRiotResult.cshtml** - Removed Blade comments, made elements clickable
5. **Player/Index.cshtml** - Player names link to SearchRiot
6. **Champion/Index.cshtml** - Manual search button, English text
7. **Champion/FromGames.cshtml** - New view created
8. **Shared/_Layout.cshtml** - Reorganized sidebar navigation
9. **Home/Index.cshtml** - Display full match IDs

---

## 📊 Statistics

- **Files Modified**: 15+
- **New Files Created**: 1 (Champion/FromGames.cshtml)
- **Old Documentation Removed**: 3 files
- **Champions Auto-Seeded**: 1700+ (from Riot DDragon API)
- **New Features**: 5+
- **Bug Fixes**: 8+

---

## 🚀 Next Steps (If Needed)

- Test rank fetching with actual ranked players
- Verify SearchRiot player history doesn't save to "All Players"
- Monitor database seeding performance with large champion counts
- Consider pagination for Champion lists if they grow too large

---

**Completed Date**: May 19, 2026
**Status**: ✅ READY FOR TESTING
