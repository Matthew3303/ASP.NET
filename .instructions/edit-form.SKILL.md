---
name: Edit-Form-Skill
description: Skill za kreiranje Edit/Create formi za EF modele
applies-to:
  files:
    - "Views/**/*.cshtml"
  patterns:
    - "edit form"
    - "create form"
    - "form page"
---

# ★ LAB3 - Edit Form Skill

Specijalizirani skill za kreiranje Edit/Create formi za EF modele u PostMatchSummary aplikaciji.

## Kada koristiti ovaj skill

- Trebam kreirati novu Edit stranicu za model
- Trebam kreirati Create stranicu za dodavanje nove stavke
- Trebam ažurirati Edit formu s novim poljima
- Trebam validirati podatke u formi

## Struktura Edit/Create Forme

### 1. Controller - Create Akcija (GET)

```csharp
// Prikazi praznu formu
[Route("create")]
[HttpGet]
public IActionResult Create()
{
    return View();
}

// Prihvati podatke iz forme
[Route("create")]
[HttpPost]
public IActionResult Create(Player player)
{
    if (ModelState.IsValid)
    {
        _context.Players.Add(player);
        _context.SaveChanges();
        return RedirectToAction("Details", new { id = player.Id });
    }
    return View(player);
}
```

### 2. Controller - Edit Akcija (GET/POST)

```csharp
// Prikazi formu s postojećim podacima
[Route("edit/{id}")]
[HttpGet]
public IActionResult Edit(int id)
{
    var player = _context.Players.Find(id);
    if (player == null) return NotFound();
    return View(player);
}

// Spremi izmijenjene podatke
[Route("edit/{id}")]
[HttpPost]
public IActionResult Edit(int id, Player player)
{
    if (id != player.Id) return BadRequest();
    
    if (ModelState.IsValid)
    {
        _context.Players.Update(player);
        _context.SaveChanges();
        return RedirectToAction("Details", new { id = player.Id });
    }
    return View(player);
}
```

### 3. View - Edit Form (.cshtml)

```html
@model PostMatchSummary.Models.Player

@{
    ViewData["Title"] = "Uredi Igrača";
}

<div class="container mt-5">
    <h2>@ViewData["Title"]</h2>
    
    <form method="post" action="/players/edit/@Model.Id">
        <div class="form-group">
            <label for="summonerName">Summoner Name:</label>
            <input type="text" class="form-control" id="summonerName" 
                   asp-for="SummonerName" required />
        </div>

        <div class="form-group">
            <label for="kills">Kills:</label>
            <input type="number" class="form-control" id="kills" 
                   asp-for="Kills" min="0" required />
        </div>

        <div class="form-group">
            <label for="deaths">Deaths:</label>
            <input type="number" class="form-control" id="deaths" 
                   asp-for="Deaths" min="0" required />
        </div>

        <div class="form-group">
            <label for="assists">Assists:</label>
            <input type="number" class="form-control" id="assists" 
                   asp-for="Assists" min="0" required />
        </div>

        <button type="submit" class="btn btn-primary">Spremi</button>
        <a href="@Url.Action("Index")" class="btn btn-secondary">Otkaži</a>
    </form>
</div>
```

## HTML Form Tag Helper-i

Koristi ASP.NET Tag Helper-e za automatsku generaciju input elemenata:

| Tag Helper | Opis |
|------------|------|
| `<input asp-for="Property" />` | Generira input s ispravnom tipom |
| `<textarea asp-for="Property" />` | Generira textarea |
| `<select asp-for="Property" asp-items="@ViewBag.Items" />` | Generira dropdown |
| `<span asp-validation-for="Property" />` | Prikazuje validation poruke |

## Partial View za Formu

Ako se forma koristi na više mjesta, kreiraj partial:

**_PlayerForm.cshtml**:
```html
@model PostMatchSummary.Models.Player

<form method="post">
    <div class="form-group">
        <label asp-for="SummonerName"></label>
        <input type="text" class="form-control" asp-for="SummonerName" />
        <span asp-validation-for="SummonerName" class="text-danger"></span>
    </div>
    <!-- ... više polja ... -->
    <button type="submit" class="btn btn-primary">Spremi</button>
</form>
```

**Create.cshtml**:
```html
@model PostMatchSummary.Models.Player

@{
    ViewData["Title"] = "Kreiraj novog igrača";
}

<div class="container">
    <h1>@ViewData["Title"]</h1>
    <partial name="_PlayerForm" model="Model" />
</div>
```

## Validacija

### Data Annotations
```csharp
public class Player
{
    [Required(ErrorMessage = "Ime je obavezno")]
    [StringLength(50, MinimumLength = 3)]
    public string SummonerName { get; set; }

    [Range(0, 100)]
    public int Kills { get; set; }
}
```

### ModelState u Controlleru
```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState.Values.SelectMany(v => v.Errors);
    return View(player); // Prikaži formu s greškama
}
```

## Best Practices

1. **Koristiti POST za izmjene**: GET samo za prikaz forme, POST za spremanje
2. **Redirect after POST**: Nakon uspješnog POST-a, pressmjeri na drugu stranicu
3. **Validation**: Validiraj na klijentskoj i serverskoj strani
4. **CSRF zaštita**: `<input type="hidden" asp-for="Model.Id" />` 
5. **Partial forme**: Koristi partial za DRY princip

## Vježba: Dodaj Create formu za Player

**Zadatak**: Kreiraj Create akciju i formu za dodavanje novog igrača.

**Rješenje**:
1. Dodaj u PlayerController:
```csharp
[Route("kreiraj")]
[HttpGet]
public IActionResult Create() => View();

[Route("kreiraj")]
[HttpPost]
public IActionResult Create(Player player)
{
    _context.Players.Add(player);
    _context.SaveChanges();
    return RedirectToAction("Details", new { name = player.SummonerName });
}
```

2. Kreiraj `Views/Player/Create.cshtml` s formom

3. Dodaj link u Index:
```html
<a href="/igraci/kreiraj" class="btn btn-success">Dodaj igrača</a>
```

## Related Skills

- EF-Model-Skill: Za rad s EF modelima
- List-Page-Skill: Za prikaz svih stavki nakon kreiranja
- UX-Agent: Za HTML/CSS strukturu forme

## Lab 3 Bodovi

**Izrada edit form stranice** (dio 1 boda):
- ✅ Razumijevanje HTTP metoda (GET/POST)
- ✅ Rad s EF podatcima (Add, Update, SaveChanges)
- ✅ Validacija podataka
- ✅ Tag Helper-i za form elemente
- ✅ Rad s ModelState-om
