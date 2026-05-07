# ★ LAB3 - Semantički Model Baze Podataka

Sažetan pregled strukture baze podataka, entiteta, svojstava i veza među tablicama.

## Entiteti i Tablice

### 1. **Match** (Utakmica)
- **Primarni ključ**: `Id` (int, auto-increment)
- **Svojstva**:
  - `MatchId` (string, unique) - Jedinstveni Riot API identifikator utakmice
  - `GameCreation` (datetime2) - Vrijeme početka utakmice
  - `Duration` (int) - Trajanje utakmice u sekundama
  - `GameMode` (string) - Tip igranja (npr. "CLASSIC", "ARAM")
  - `GameVersion` (string) - Verzija igre
- **Relacije**:
  - 1-N: Match → Team (jedan Match ima više Teams)
  - 1-N: Match → Player (jedan Match ima više Players)
- **Pomoćna svojstva** (NotMapped):
  - `WinnerTeam` - Timovi koji su pobjedili
  - `TotalKills` - Ukupan broj ubijenih neprijatelja

---

### 2. **Team** (Tim)
- **Primarni ključ**: `Id` (int, auto-increment)
- **Svojstva**:
  - `TeamId` (int) - Logički identifikator (100=Blue, 200=Red)
  - `Win` (bool) - Je li tim pobijedio
  - `MatchId` (int, nullable, FK) - Strani ključ na Match
- **Relacije**:
  - N-1: Team → Match (više Teams pripada jednom Match-u)
  - 1-N: Team → Player (jedan Team ima više Players)
- **Pomoćna svojstva** (NotMapped):
  - `TeamName` (string) - Ljudski čitljiv naziv ("Blue Team" / "Red Team")
  - `TotalKills` - Zbir svih ubijenih od strane igrača u timu
  - `TotalGold` - Zbir svih zarađenih zlata od strane igrača u timu
- **Brisanje**: Cascade - ako se obriše Match, brišu se i timovi

---

### 3. **Player** (Igrač)
- **Primarni ključ**: `Id` (int, auto-increment)
- **Svojstva**:
  - `SummonerName` (string) - Korisničko ime na Riot platformi
  - `ChampionId` (int, FK) - Strani ključ na Champion
  - `Kills` (int) - Broj ubijenih neprijatelja
  - `Deaths` (int) - Broj umrlih
  - `Assists` (int) - Broj asistencija
  - `CS` (int) - Creep Score (broj ubijenih neutan moba)
  - `GoldEarned` (int) - Zaradjeno zlato
  - `Win` (bool) - Je li igrač pobijedio
  - `Role` (ChampionRole enum) - Pozicija na kojoj je igrao (Top, Jungle, Mid, ADC, Support, Unknown)
  - `TeamId` (int, nullable, FK) - Strani ključ na Team
  - `MatchId` (int, nullable, FK) - Strani ključ na Match
- **Relacije**:
  - N-1: Player → Team (više Players pripada jednom Team-u)
  - N-1: Player → Champion (više Players igra istog Championona)
  - N-1: Player → Match (više Players sudjeluje u jednom Match-u)
- **Brisanje**:
  - Team: SetNull - ako se obriše Team, TeamId se postavlja na null
  - Champion: Restrict - ne dozvoli brisanje Champion-a ako ga igrači koriste
  - Match: Cascade - ako se obriše Match, brišu se i igrači

---

### 4. **Champion** (Junak/Lik)
- **Primarni ključ**: `Id` (int, auto-increment)
- **Svojstva**:
  - `Name` (string) - Naziv junaka (npr. "Aatrox", "Ahri")
- **Relacije**:
  - 1-N: Champion → Player (jedan Champion se može igrati od više Players)
- **Pomoćna svojstva**: Nema

---

## Dijagram Veza (Relationships)

```
┌─────────────────────┐
│      MATCH          │
│  (Utakmica)         │
├─────────────────────┤
│ Id (PK)             │
│ MatchId (Unique)    │
│ GameCreation        │
│ Duration            │
│ GameMode            │
│ GameVersion         │
└──────────┬──────────┘
           │
           │ 1-N
           │
    ┌──────▼──────────────────────────────────┐
    │                                          │
    ▼                                          ▼
┌─────────────┐                      ┌──────────────┐
│   TEAM      │                      │   PLAYER     │
│  (Tim)      │                      │  (Igrač)     │
├─────────────┤                      ├──────────────┤
│ Id (PK)     │                      │ Id (PK)      │
│ TeamId      │                      │ SummonerName │
│ Win         │ ◄────────────────────│ ChampionId   │
│ MatchId(FK) │      1-N (SetNull)   │ Kills        │
└─────────────┘                      │ Deaths       │
    │                                │ Assists      │
    │ 1-N                            │ CS           │
    │ (Cascade)                      │ GoldEarned   │
    │                                │ Win          │
    │                                │ Role         │
    └───────────────────────┬────────│ TeamId (FK)  │
                            │        │ MatchId (FK) │
                            └────────└──────────────┘
                                      │
                                      │ N-1 (Restrict)
                                      │
                            ┌─────────▼──────────┐
                            │    CHAMPION        │
                            │   (Junak/Lik)     │
                            ├────────────────────┤
                            │ Id (PK)            │
                            │ Name               │
                            └────────────────────┘
```

---

## Svojstva Veza

| Veza | Tip | Foreign Key | Delete Behavior | Napomena |
|------|-----|-------------|-----------------|----------|
| Match → Team | 1-N | Team.MatchId | **Cascade** | Ako se obriše Match, brišu se svi timovi |
| Match → Player | 1-N | Player.MatchId | **Cascade** | Ako se obriše Match, brišu se svi igrači |
| Team → Player | 1-N | Player.TeamId | **SetNull** | Ako se obriše Team, Player.TeamId → null |
| Champion → Player | 1-N | Player.ChampionId | **Restrict** | Ne dozvoli brisanje Champion-a ako se koristi |

---

## Inicijalni Podaci

Baza se inicijalizuje s:
- **Match** - Učitavaju se iz Riot API-ja na osnovu `SeedMatchIds` iz `appsettings.json`
- **Team** - Kreiraju se automatski pri učitavanju Match podataka
- **Player** - Kreiraju se automatski pri učitavanju Match podataka
- **Champion** - Kreiraju se pri učitavanju Player podataka ako ne postoje

Detaljnije: [Program.cs](Program.cs#L18-L28)
