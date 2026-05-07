# ★ LAB3 - Semantički Routing Model (Sitemap)

Detaljni popis svih dostupnih URL-ova u aplikaciji, s informacijom o Controller-u, akciji i korištenim View-ovima.

---

## 1. Home Controller - Homepage

| # | URL | Ruta | Controller | Akcija | View | Opis |
|---|-----|------|-----------|--------|------|------|
| 1 | `/` | `""` | Home | **Index** | `Views/Home/Index.cshtml` | Homepage - displays match cache |
| 2 | `/home` | `"home"` | Home | **Index** | `Views/Home/Index.cshtml` | Alternative homepage route |
| 3 | `/home/index` | `"home/index"` | Home | **Index** | `Views/Home/Index.cshtml` | Standard homepage route |
| 4 | `/home/error` | `"home/error"` | Home | **Error** | `Views/Home/Error.cshtml` | Error page for displaying errors |
| 5 | `/error` | `"error"` | Home | **Error** | `Views/Home/Error.cshtml` | Alternative error route |
| 6 | `/about` | `"about"` | Home | **About** | `Views/Home/About.cshtml` | ★ LAB3 - New action: About the app |

---

## 2. Player Controller - Players

| # | URL | Ruta | Controller | Akcija | View | Opis |
|---|-----|------|-----------|--------|------|------|
| 7 | `/api/players` | `"api/players"` | Player | **Index** | `Views/Player/Index.cshtml` | ★ LAB3 - List of all players |
| 8 | `/api/players/list` | `"api/players/list"` | Player | **Index** | `Views/Player/Index.cshtml` | ★ LAB3 - Alternative list route |
| 9 | `/api/players/{name}` | `"api/players/{name}"` | Player | **Details** | `Views/Player/Details.cshtml` | ★ LAB3 - Player details by name |
| 10 | `/api/players/profile/{name}` | `"api/players/profile/{name}"` | Player | **Details** | `Views/Player/Details.cshtml` | ★ LAB3 - Player profile |
| 11 | `/api/players/statistics/{name}` | `"api/players/statistics/{name}"` | Player | **Stats** | `Views/Player/Details.cshtml` | ★ LAB3 - New action: Player statistics |
| 12 | `/api/players/stats/{name}` | `"api/players/stats/{name}"` | Player | **Stats** | `Views/Player/Details.cshtml` | ★ LAB3 - Alternative stats route |

---

## 3. PostMatch Controller - Matches

| # | URL | Ruta | Controller | Akcija | View | Opis |
|---|-----|------|-----------|--------|------|------|
| 13 | `/api/matches` | `"api/matches"` | PostMatch | **Index** | `Views/PostMatch/Index.cshtml` | ★ LAB3 - List of matches |
| 14 | `/api/matches/overview` | `"api/matches/overview"` | PostMatch | **Index** | `Views/PostMatch/Index.cshtml` | ★ LAB3 - Overview of matches |
| 15 | `/api/matches/{matchId}` | `"api/matches/{matchId}"` | PostMatch | **Details** | `Views/PostMatch/Details.cshtml` | ★ LAB3 - Match details by ID |
| 16 | `/api/matches/json/{matchId}` | `"api/matches/json/{matchId}"` | PostMatch | **RawJson** | `Views/PostMatch/RawJson.cshtml` | ★ LAB3 - Raw JSON from Riot API |
| 17 | `/api/matches/raw-json/{matchId}` | `"api/matches/raw-json/{matchId}"` | PostMatch | **RawJson** | `Views/PostMatch/RawJson.cshtml` | ★ LAB3 - Alternative JSON route |
| 18 | `/api/matches/top` | `"api/matches/top"` | PostMatch | **TopMatches** | `Views/PostMatch/Index.cshtml` | ★ LAB3 - New action: Top 5 matches |
| 19 | `/api/matches/trending` | `"api/matches/trending"` | PostMatch | **TopMatches** | `Views/PostMatch/Index.cshtml` | ★ LAB3 - Alternative trending route |

---

## Legenda

| Oznaka | Značenje |
|--------|----------|
| ★ LAB3 | Akcija/Ruta dodana ili ažurirana u Lab 3 |
| `{name}` | Parameter - Summoner name (string) |
| `{matchId}` | Parameter - Match ID iz Riot API-ja (string) |

---

## Organizacija View-ova

```
Views/
├── Home/
│   ├── Index.cshtml          ← Index akcija
│   ├── Error.cshtml          ← Error akcija
│   └── About.cshtml          ← ★ LAB3 - Nova About akcija
├── Player/
│   ├── Index.cshtml          ← Index akcija (lista igrača)
│   └── Details.cshtml        ← Details i Stats akcije
├── PostMatch/
│   ├── Index.cshtml          ← Index i TopMatches akcije
│   ├── Details.cshtml        ← Details akcija
│   └── RawJson.cshtml        ← RawJson akcija
└── Shared/
    ├── _Layout.cshtml        ← Glavna layout stranica
    └── Error.cshtml          ← Greška
```

---

## Flow aplikacije

### 1. Početak → Početna Stranica
```
GET /
  ↓
HomeController.Index()
  ↓
Views/Home/Index.cshtml (prikazuje MatchCache)
```

### 2. Pretraga Igrača
```
GET /igraci
  ↓
PlayerController.Index()
  ↓
Views/Player/Index.cshtml (lista svih igrača)
  ↓
Klik na igrača
  ↓
GET /igraci/{name}
  ↓
PlayerController.Details(name)
  ↓
Views/Player/Details.cshtml (detalji igrača)
```

### 3. Pretraga Utakmica
```
GET /mecevi
  ↓
PostMatchController.Index()
  ↓
Views/PostMatch/Index.cshtml (lista utakmica)
  ↓
Klik na utakmicu
  ↓
GET /mecevi/{matchId}
  ↓
PostMatchController.Details(matchId)
  ↓
Views/PostMatch/Details.cshtml (detalji utakmice)
```

### 4. Top Utakmice
```
GET /mecevi/top
  ↓
PostMatchController.TopMatches()
  ↓
Views/PostMatch/Index.cshtml (top 5 po kills)
```

---

## Napomena: Custom Routing

Sve akcije u aplikaciji koriste **[Route]** atribute umjesto default ASP.NET MVC pattern-a:
- ★ LAB3 kontrola: Svaka akcija ima **barem 2 različite rute** koja se može aktivirati
- Primjer: `PlayerController.Index()` može biti dostupna na `/igraci`, `/igraci/lista`, ili `/api/players`

Ovo omogućava većoj fleksibilnosti i SEO-optimizaciji URL-ova.
