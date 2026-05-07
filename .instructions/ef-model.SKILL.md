---
name: EF-Model-Skill
description: Skill za dodavanje novih EF modela, modifikaciju svojstava ili generiranje migracija
applies-to:
  files:
    - "Models/**/*.cs"
    - "**/DbContext.cs"
  patterns:
    - "entity-framework"
    - "ef-model"
    - "migration"
---

# ★ LAB3 - EF Model Skill

Specijializirani skill za rad s Entity Framework modelima u PostMatchSummary projektu.

## Kada koristiti ovaj skill

- Trebam dodati novo svojstvo u postojeći EF model (Player, Match, Team, Champion)
- Trebam kreirati novi EF model/entitet
- Trebam promijeniti veze između modela (1-N, N-N)
- Trebam generirati novu migraciju nakon promjene modela
- Trebam ažurirati DbContext relacije

## Koraci

### 1. Dodaj nove svojstva ili klase
Ako trebam dodati novo svojstvo u model, trebam:
- Dodati svojstvo s odgovarajućim tipom podataka
- Ako je strani ključ: dodati `[ForeignKey("RelationName")]` anotaciju
- Ako je kolekcija: koristiti `virtual ICollection<T>`
- Dodati `using System.ComponentModel.DataAnnotations;`
- Dodati `using System.ComponentModel.DataAnnotations.Schema;`

Primjer novog svojstva u Player:
```csharp
[ForeignKey("Team")]
public int? TeamId { get; set; }
public virtual Team? Team { get; set; }
```

### 2. Ažuriraj DbContext (PostMatchSummaryDbContext.cs)
U `OnModelCreating` metodi dodaj konfiguraciju za novu vezu:
```csharp
modelBuilder.Entity<Player>()
    .HasOne(p => p.Team)
    .WithMany(t => t.Players)
    .HasForeignKey(p => p.TeamId)
    .OnDelete(DeleteBehavior.SetNull);
```

### 3. Generiraj migraciju
U Developer PowerShell konzoli:
```powershell
cd PostMatchSummary
dotnet ef migrations add MigrationName --startup-project .
```

### 4. Primijeni migraciju
```powershell
dotnet ef database update --startup-project .
```

## Postojeći Modeli

### Player.cs
- [x] [Key] Id
- [x] [ForeignKey] ChampionId
- [x] [ForeignKey] TeamId  
- [x] [ForeignKey] MatchId
- [x] virtual Champion
- [x] virtual Team
- [x] virtual Match

### Match.cs
- [x] [Key] Id
- [x] Unique index na MatchId
- [x] virtual ICollection<Team>
- [x] virtual ICollection<Player>

### Team.cs
- [x] [Key] Id
- [x] [ForeignKey] MatchId
- [x] virtual Match
- [x] virtual ICollection<Player>

### Champion.cs
- [x] [Key] Id
- [x] virtual ICollection<Player>

## DeleteBehavior Options

| Opcija | Ponašanje |
|--------|-----------|
| **Cascade** | Obriši relatirane zapise (obriši Match → obriši sve Teams i Players) |
| **SetNull** | Postavi FK na null (obriši Team → Player.TeamId = null) |
| **Restrict** | Zabrani brisanje (ne možeš obrisati Champion ako ga igrači koriste) |
| **NoAction** | Nema akcije (SQL će baciti grešku) |

## Vježba: Dodaj novo svojstvo

**Zadatak**: Dodaj `Level` (int) svojstvo u Player model koji predstavlja razinu igrača.

**Rješenje**:
1. Otvori [Models/Player.cs](PostMatchSummary/Models/Player.cs)
2. Dodaj: `public int Level { get; set; }`
3. U Developer PowerShell: `dotnet ef migrations add AddPlayerLevel`
4. `dotnet ef database update`

## Povezani Skillovi

- List-Page-Skill: Za prikaz podataka nakon što su dodani u EF
- Edit-Form-Skill: Za enformiranje podataka u formi

## Lab 3 Bodovi

**Konfiguracija EF u projektu** (1 bod):
- ✅ Ispravne anotacije na modelima ([Key], [ForeignKey])
- ✅ Virtual svojstva za 1-N veze
- ✅ Instalirana i konfigurirana baza podataka
- ✅ DbContext i DI konfiguracija
- ✅ Detaljnije: [PostMatchSummaryDbContext.cs](PostMatchSummary/PostMatchSummaryDbContext.cs)

**Razumijevanje EF principa** (1 bod):
- Ovaj skill pokazuje razumijevanje veza, ključeva i migracija
