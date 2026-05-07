---
name: Routing-Skill
description: Skill za rad s ASP.NET Core routing-om i [Route] atributima
applies-to:
  files:
    - "Controllers/**/*.cs"
  patterns:
    - "routing"
    - "route"
    - "[Route]"
---

# ★ LAB3 - Routing Skill

Specijalizirani skill za rad s ASP.NET Core routing-om u PostMatchSummary projektu.

## Kada koristiti ovaj skill

- Trebam kreirati custom rutu za akciju
- Trebam dodati multiple rute na jednu akciju
- Trebam koristiti route constraints (int, length, regex)
- Trebam razumjeti razliku između default i custom routing-a
- Trebam kreirati API rute

## Osnovno o Routing-u

### Default Routing (u Program.cs)
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Aktivira se na: `/Home/Index`, `/Home/Index/123`, etc.

### Custom Routing s [Route] atributima

**Attribute na akciji**:
```csharp
[Route("custom-path")]
public IActionResult MyAction() { }
```

**Attribute na controlleru + akciji**:
```csharp
[Route("api/players")]
public class PlayerController : Controller
{
    [Route("")]           // /api/players
    [Route("list")]       // /api/players/list
    public IActionResult Index() { }
}
```

## Primjeri Custom Routing-a u Projektu

### HomeController

```csharp
[Route("")]              // /
[Route("home")]          // /home
[Route("home/index")]    // /home/index
public IActionResult Index() { }

[Route("about")]         // /about
[Route("o-nama")]        // /o-nama (lokalizirana verzija)
public IActionResult About() { }
```

### PlayerController

```csharp
[Route("api/players")]
[Route("igraci")]
public class PlayerController : Controller
{
    [Route("")]              // /igraci ili /api/players
    [Route("lista")]         // /igraci/lista
    public IActionResult Index() { }

    [Route("{name}")]        // /igraci/IgnoreName
    [Route("profil/{name}")] // /igraci/profil/IgnoreName
    public IActionResult Details(string name) { }

    [Route("statistika/{name}")]  // /igraci/statistika/IgnoreName
    [Route("stats/{name}")]       // /igraci/stats/IgnoreName
    public IActionResult Stats(string name) { }
}
```

### PostMatchController

```csharp
[Route("api/postmatch")]
[Route("mecevi")]
public class PostMatchController : Controller
{
    [Route("")]              // /mecevi ili /api/postmatch
    [Route("pregled")]       // /mecevi/pregled
    public IActionResult Index() { }

    [Route("{matchId}")]     // /mecevi/EUW1_123456
    public IActionResult Details(string matchId) { }

    [Route("json/{matchId}")] // /mecevi/json/EUW1_123456
    [Route("raw-json/{matchId}")] // /mecevi/raw-json/EUW1_123456
    public IActionResult RawJson(string matchId) { }

    [Route("top")]           // /mecevi/top
    [Route("najbolji")]      // /mecevi/najbolji
    public IActionResult TopMatches() { }
}
```

## Route Constraints

Dodaj constraints za parametar:

```csharp
[Route("players/{id:int}")]           // id mora biti int
[Route("champions/{name:length(3,20)}")] // name 3-20 znakova
[Route("matches/{id:guid}")]          // id mora biti GUID
[Route("season/{year:range(2010,2025)}")] // year između 2010-2025
public IActionResult Action(string param) { }
```

Dostupni constraints:
- `:int`, `:long`, `:guid`, `:bool`, `:float`, `:double`, `:decimal`
- `:length(min,max)`, `:minlength(value)`, `:maxlength(value)`
- `:alpha`, `:regex(pattern)`, `:required`

## HTTP Metode

Koristi `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`:

```csharp
[HttpGet("players/{id}")]
public IActionResult GetPlayer(int id) { }

[HttpPost("players")]
public IActionResult CreatePlayer(Player player) { }

[HttpPut("players/{id}")]
public IActionResult UpdatePlayer(int id, Player player) { }

[HttpDelete("players/{id}")]
public IActionResult DeletePlayer(int id) { }
```

## Rute s Optionalnim Parametrima

```csharp
[Route("search/{query?}")]  // query je opcionalan
public IActionResult Search(string? query) { }

// Pristupačno na:
// /search
// /search/aatrox
```

## Rute s Više Parametara

```csharp
[Route("season/{year:int}/team/{teamName}")]
public IActionResult SeasonTeam(int year, string teamName) { }

// /season/2024/team/FNC
```

## RESTful Routing Primjer

```csharp
[Route("api/v1/players")]
public class PlayerController : Controller
{
    [HttpGet]                    // GET /api/v1/players
    public IActionResult GetAll() { }

    [HttpGet("{id}")]            // GET /api/v1/players/5
    public IActionResult GetById(int id) { }

    [HttpPost]                   // POST /api/v1/players
    public IActionResult Create(Player player) { }

    [HttpPut("{id}")]            // PUT /api/v1/players/5
    public IActionResult Update(int id, Player player) { }

    [HttpDelete("{id}")]         // DELETE /api/v1/players/5
    public IActionResult Delete(int id) { }
}
```

## Vježba: Dodaj custom rutu

**Zadatak**: Dodaj novu akciju `AdvancedSearch` sa rutama:
- `/igraci/pretraga-napredna`
- `/igraci/advanced-search`

**Rješenje**:
```csharp
[Route("pretraga-napredna")]
[Route("advanced-search")]
public IActionResult AdvancedSearch()
{
    return View();
}
```

## Best Practices

1. **Konvencija**: Imena routera trebaju biti opisna (npr. `new-player`, `edit-match`)
2. **Dosljednost**: Koristi iste konvencije kroz projekt (kebab-case za URL-e)
3. **Versioning**: API rute trebale imati verziju (`/api/v1/players`)
4. **Semantika**: Koristi HTTP metode ispravno (GET za čitanje, POST za pisanje)
5. **Dokumentacija**: Dokumentiraj custom rute (kao što je urađeno ovdje)

## Program.cs Konfiguracija

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Ova ruta je fallback - ako nijedna `[Route]` anotacija ne odgovara, koristi se ova.

## Related Skills

- EF-Model-Skill: Za rad s modelima
- List-Page-Skill: Za prikaz podataka
- Edit-Form-Skill: Za forme

## Lab 3 Bodovi

**Podesiti custom routing na barem 4 akcije** (1 bod):
- ✅ HomeController:
  - Index: `/`, `/home`, `/home/index`
  - Error: `/home/greska`
  - About: `/about`, `/o-nama` (LAB3)
  
- ✅ PlayerController:
  - Index: `/igraci`, `/igraci/lista`, `/api/players`
  - Details: `/igraci/{name}`, `/igraci/profil/{name}`
  - Stats: `/igraci/statistika/{name}`, `/igraci/stats/{name}` (LAB3)
  
- ✅ PostMatchController:
  - Index: `/mecevi`, `/mecevi/pregled`, `/api/postmatch`
  - Details: `/mecevi/{matchId}`
  - RawJson: `/mecevi/json/{matchId}`, `/mecevi/raw-json/{matchId}`
  - TopMatches: `/mecevi/top`, `/mecevi/najbolji` (LAB3)

**Svaka akcija ima minimum 2 različite rute** ✅
