---
name: List-Page-Skill
description: Skill za kreiranje List (Index) stranica za prikaz podataka iz EF modela
applies-to:
  files:
    - "Views/**/*.cshtml"
  patterns:
    - "list page"
    - "index page"
    - "index.cshtml"
---

# ★ LAB3 - List Page Skill

Specijalizirani skill za kreiranje list/index stranica u PostMatchSummary aplikaciji.

## Kada koristiti ovaj skill

- Trebam kreirati novu Index/List stranicu
- Trebam ažurirati postojeću Index stranicu
- Trebam prikazati kolekciju podataka iz EF-a (List<T>)
- Trebam dodati filtriranje/sortiranje na list stranici

## Struktura List Stranice

### 1. Model
```csharp
// Controllers/PlayerController.cs
[Route("igraci")]
public class PlayerController : Controller
{
    [Route("")]
    [Route("lista")]
    public IActionResult Index()
    {
        var playersWithMatches = _cache.GetAllPlayersWithMatches();
        return View(playersWithMatches);  // ← Model je List<Player>
    }
}
```

### 2. View (.cshtml)

Osnovna struktura:
```html
@model List<PostMatchSummary.Models.Player>

@{
    ViewData["Title"] = "Lista Igrača";
}

<div class="container">
    <h1>@ViewData["Title"]</h1>
    
    @if (Model != null && Model.Count > 0)
    {
        <table class="table">
            <thead>
                <tr>
                    <th>Ime</th>
                    <th>Tim</th>
                    <th>Kill/Deaths/Assists</th>
                    <th>Akcije</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var player in Model)
                {
                    <tr>
                        <td>@player.SummonerName</td>
                        <td>@player.Team?.TeamName</td>
                        <td>@player.Kills / @player.Deaths / @player.Assists</td>
                        <td>
                            <a href="/igraci/@player.SummonerName" class="btn btn-sm btn-primary">
                                Detalji
                            </a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }
    else
    {
        <p>Nema dostupnih podataka.</p>
    }
</div>
```

## Primjeri List Stranica u Projektu

### PlayerController.Index
- **View**: [Views/Player/Index.cshtml](PostMatchSummary/Views/Player/Index.cshtml)
- **Model**: `List<Player>`
- **Prikazuje**: Sve igrače sa njihovim statistikom
- **Akcije**: Klik → Detalji igrača

### PostMatchController.Index
- **View**: [Views/PostMatch/Index.cshtml](PostMatchSummary/Views/PostMatch/Index.cshtml)
- **Model**: `List<Match>`
- **Prikazuje**: Sve utakmice sa datumima i moderama
- **Akcije**: Klik → Detalji utakmice

## Best Practices za List Stranice

### 1. Paginacija (ako ima puno podataka)
```csharp
@{
    var pageSize = 10;
    var pageNumber = ViewBag.PageNumber ?? 1;
    var totalPages = (int)Math.Ceiling(Model.Count / (double)pageSize);
}

@Html.PagedListPager(Model, pageNumber)
```

### 2. Sortiranje
```csharp
<th>
    <a href="/igraci?sort=kills">Kill</a>
</th>
```

### 3. Filtriranje
```html
<form method="get" action="/igraci">
    <input type="text" name="search" placeholder="Pretraži igrače..." />
    <button type="submit">Pretraži</button>
</form>
```

### 4. Responsive Tablica
```html
<div class="table-responsive">
    <table class="table">
        <!-- ... -->
    </table>
</div>
```

## Vježba: Kreiraj novu List stranicu

**Zadatak**: Kreiraj list stranicu za sve Champion-e s brojem igrača koji ih koriste.

**Rješenje**:
1. Kreiraj novu akciju `ListChampions` u `HomeController`
2. Model: `List<Champion>`
3. Prikaži tablicu s:
   - Naziv Champion-a
   - Broj igrača koji ga koriste (`champion.Players.Count`)
   - Link na detaljnu stranicu

## Related Skills

- EF-Model-Skill: Za rad s modelima i podacima
- Edit-Form-Skill: Za enformiranje/kreiranje podataka
- UX-Agent: Za HTML i CSS strukturu

## Lab 3 Bodovi

**Izrada list stranice koristeći skill** (dio 1 boda):
- ✅ List stranice za Player i Match su postojeće
- ✅ Koriste EF podatke (@Model)
- ✅ Prikazuju relacije (Team, Champion)
- ✅ Imaju navigaciju (linkovi na Detalji)
- Detaljnije: [Views/Player/Index.cshtml](PostMatchSummary/Views/Player/Index.cshtml)
