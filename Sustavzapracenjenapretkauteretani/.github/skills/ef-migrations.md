# Skill: Entity Framework — migracije i modeliranje

Koristi ovaj skill svaki put kad trebaš promijeniti model, dodati migraciju ili osvježiti bazu podataka.

---

## Kada koristiti

- Dodavanje novog polja na postojeću klasu/tablicu
- Dodavanje nove klase (nove tablice)
- Dodavanje ili izmjena veza između entiteta
- Dodavanje seed podataka (`HasData`)
- Brisanje polja ili tablice

---

## Struktura projekta (relevantni folderi)

```
Models/                        ← domenski modeli (klase = tablice)
DAL/TeretanaDbContext.cs       ← DbContext s DbSet<T> za svaki entitet
DAL/Repositories/              ← repozitoriji koji koriste DbContext
Migrations/                    ← automatski generirane migracijske datoteke
```

---

## Korak 1 — Promjena modela

Promijeni klasu u `Models/`. Primjeri:

### Dodavanje novog polja

```csharp
// Models/Vjezba.cs
[StringLength(200)]
[Display(Name = "Napomena")]
public string? Napomena { get; set; }
```

### Dodavanje nove klase (nova tablica)

```csharp
// Models/NoviEntitet.cs
public class NoviEntitet
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Naziv { get; set; } = string.Empty;

    public int KorisnikId { get; set; }
    public virtual Korisnik Korisnik { get; set; } = null!;
}
```

### Dodavanje veze između entiteta

```csharp
// U roditeljskom modelu (npr. Korisnik.cs)
public virtual ICollection<NoviEntitet> NoviEntiteti { get; set; } = new HashSet<NoviEntitet>();
```

---

## Korak 2 — Registracija u DbContext

Ako je dodana **nova klasa**, dodaj `DbSet<T>` u `DAL/TeretanaDbContext.cs`:

```csharp
public DbSet<NoviEntitet> NoviEntiteti { get; set; }
```

Ako je samo dodano polje na postojeću klasu — ovaj korak nije potreban.

---

## Korak 3 — Seed podaci (opcionalno)

Seed podaci se dodaju u metodu `OnModelCreating` unutar `DAL/TeretanaDbContext.cs`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Vjezba>().HasData(
        new Vjezba { Id = 1, Naziv = "Bench Press", MisicnaSkupina = MisicnaSkupina.Prsa, RazinaTezine = 3, PotrebnaOprema = true },
        new Vjezba { Id = 2, Naziv = "Čučnjevi",    MisicnaSkupina = MisicnaSkupina.Noge,  RazinaTezine = 2, PotrebnaOprema = false }
    );
}
```

**Pravila za seed podatke:**
- `Id` mora biti eksplicitno naveden i jedinstven
- Nakon dodavanja seed podataka **obavezno kreirati novu migraciju**
- Ne mijenjaj `Id` već unesenih seed podataka — EF ih prati po Id-u

---

## Korak 4 — Kreiranje migracije

Otvori **Developer PowerShell** i pozicioniraj se u korijeni folder projekta:

```powershell
dotnet ef migrations add NazivMigracije
```

Ako projekt ima više slojeva (DAL odvojen od Web projekta):

```powershell
dotnet ef migrations add NazivMigracije --startup-project ../NazivWebProjekta --context TeretanaDbContext
```

**Konvencija naziva migracija:**

| Situacija | Primjer naziva |
|-----------|---------------|
| Inicijalna migracija | `InitialCreate` |
| Novo polje | `AddNapomentaToVjezba` |
| Nova tablica | `AddNoviEntitetTable` |
| Seed podaci | `SeedVjezbe` |
| Brisanje polja | `RemoveStaroPoljeFromKorisnik` |

---

## Korak 5 — Primjena migracije na bazu

```powershell
dotnet ef database update
```

S parametrima (višeslojni projekt):

```powershell
dotnet ef database update --startup-project ../NazivWebProjekta --context TeretanaDbContext
```

---

## Korak 6 — Generiranje SQL skripte (produkcijsko okruženje)

Umjesto direktnog `database update`, generiraj `.sql` skriptu:

```powershell
dotnet ef migrations script FROM TO --output ./Migrations/migration.sql
```

Primjer — od inicijalne do zadnje migracije:

```powershell
dotnet ef migrations script InitialCreate SeedVjezbe --output ./Migrations/seed.sql
```

---

## Poništavanje migracije

Ako migracija još **nije primijenjena** na bazu:

```powershell
dotnet ef migrations remove
```

Ako **jest primijenjena**, prvo vrati bazu na prethodnu verziju:

```powershell
dotnet ef database update PrethodnaMigracija
dotnet ef migrations remove
```

---

## Dohvat podataka s relacijama (Include)

Kad model ima navigacijska svojstva, koristiti `.Include()` u repozitoriju:

```csharp
// Jedan entitet s relacijama
var entitet = _context.Korisnici
    .Include(k => k.Treninzi)
    .Include(k => k.Ciljevi)
    .FirstOrDefault(k => k.Id == id);

// Ugniježđene relacije
var trening = _context.Treninzi
    .Include(t => t.TreningVjezbe)
        .ThenInclude(tv => tv.Vjezba)
    .FirstOrDefault(t => t.Id == id);
```

---

## Checklista

- [ ] Izmijenjen/dodan model u `Models/`
- [ ] Dodano `DbSet<T>` u `TeretanaDbContext` (samo za novu klasu)
- [ ] Dodana navigacijska svojstva na obje strane veze (ako je nova veza)
- [ ] Dodani seed podaci u `OnModelCreating` (ako je potrebno)
- [ ] Pokrenuto `dotnet ef migrations add NazivMigracije`
- [ ] Pregledana generirana migracijska datoteka u `Migrations/`
- [ ] Pokrenuto `dotnet ef database update`
- [ ] Aplikacija se uspješno pokreće i podaci su vidljivi

---

## Česti problemi

| Problem | Uzrok | Rješenje |
|---------|-------|---------|
| `No migrations configuration type was found` | Pogrešan radni direktorij | Pozicioniraj se u projekt koji sadrži `Migrations/` |
| `Unable to create an object of type 'TeretanaDbContext'` | Nedostaje connection string | Provjeri `appsettings.json` i dodaj `--startup-project` parametar |
| `The entity type requires a primary key` | Nova klasa nema `[Key]` | Dodaj `[Key]` atribut na `Id` svojstvo |
| `Microsoft.EntityFrameworkCore.Design not found` | Nedostaje NuGet paket | Dodaj `Microsoft.EntityFrameworkCore.Design` paket i rebuilaj projekt |
