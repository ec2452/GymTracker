# CRUD Operacije - Dokumentacija

## Pregled

Projekt koristi **Entity Framework Core** sa **soft delete** Pattern-om za sve osnovne operacije:
- **Create** - Kreiranje novih zapisa
- **Read** - Čitanje podataka
- **Update** - Ažuriranje zapisa
- **Delete** - Brisanje zapisa (soft delete)

---

## 1. DELETE OPERACIJA (Soft Delete)

### Što je Soft Delete?

Umjesto fizičkog brisanja zapisa iz baze, postavljamo `DeletedAt` polje na trenutno vrijeme. Obrisani zapisi ostaju u bazi ali se automatski filtriraju kod dohvaćanja.

**Prednosti:**
- ✅ Lakši oporavak podataka
- ✅ Auditni trag
- ✅ Mogućnost analitike
- ✅ Nema gubitka podataka

### Implementacija u Modelima

Svi modeli imaju `DeletedAt` polje:

```csharp
[Display(Name = "Datum brisanja")]
public DateTime? DeletedAt { get; set; }
```

Modeli sa `DeletedAt`:
- `Korisnik`
- `Cilj`
- `Trening`
- `Vjezba`
- `Mjerenje`
- `Plan`
- `TreningVjezba`

### Global Query Filters

`TeretanaDbContext` koristi **global query filters** da automatski filtrira obrisane zapise:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Automatski filtrira obrisane zapise na svim upitima
    modelBuilder.Entity<Korisnik>()
        .HasQueryFilter(k => k.DeletedAt == null);
    
    modelBuilder.Entity<Cilj>()
        .HasQueryFilter(c => c.DeletedAt == null);
    
    // ... ostali modeli
}
```

**Rezultat:** Svaki `GetAll()` ili `GetById()` automatski filtrira zapise gdje je `DeletedAt != null`.

### Soft Delete u Repository-jevima

#### Primjer - KorisnikRepository.Delete()

```csharp
public void Delete(int id)
{
    var korisnik = _context.Korisnici.FirstOrDefault(k => k.Id == id);
    if (korisnik != null)
    {
        // Soft delete - postavi DeletedAt umjesto Remove
        korisnik.DeletedAt = DateTime.UtcNow;
        _context.SaveChanges();
    }
}
```

**Ključne razlike od hard delete:**
- `FirstOrDefault()` umjesto `Find()` (global filters)
- Postavljanje `DeletedAt = DateTime.UtcNow`
- `SaveChanges()` umjesto `Remove()`

#### Hard Delete (za iznimne slučajeve)

```csharp
public void HardDelete(int id)
{
    var korisnik = _context.Korisnici
        .IgnoreQueryFilters()
        .FirstOrDefault(k => k.Id == id);
    if (korisnik != null)
    {
        // Briši sve povezane zapise
        var ciljevi = _context.Ciljevi.Where(c => c.KorisnikId == id).ToList();
        var treninzi = _context.Treninzi.Where(t => t.KorisnikId == id).ToList();
        var mjerenja = _context.Mjerenja.Where(m => m.KorisnikId == id).ToList();

        _context.Ciljevi.RemoveRange(ciljevi);
        _context.Treninzi.RemoveRange(treninzi);
        _context.Mjerenja.RemoveRange(mjerenja);
        _context.Korisnici.Remove(korisnik);
        _context.SaveChanges();
    }
}
```

### Kontroler Delete Akcije

```csharp
[HttpGet("{id:int}/{slug?}/obrisi", Name = "KorisnikDelete")]
public IActionResult Delete(int id)
{
    var korisnik = _korisnikRepository.GetById(id);
    if (korisnik is null)
        return NotFound();

    return View(korisnik);
}

[HttpPost("{id:int}/{slug?}/obrisi", Name = "KorisnikDeletePost")]
[ValidateAntiForgeryToken]
public IActionResult DeleteConfirmed(int id)
{
    _korisnikRepository.Delete(id);
    return RedirectToAction(nameof(Index));
}
```

### Relacije pri Brisanju

#### Kaskadno Brisanje (Soft Delete)

Kada se briše `Korisnik`, sve povezane `Ciljevi`, `Treninzi` i `Mjerenja` se također soft brišu:

```csharp
// TreningRepository.Delete()
public void Delete(int id)
{
    var trening = _context.Treninzi.FirstOrDefault(t => t.Id == id);
    if (trening != null)
    {
        // Soft delete treninga
        trening.DeletedAt = DateTime.UtcNow;
        
        // Soft delete sve vježbe u treningu
        var treningVjezbe = _context.TreningVjezbe
            .Where(tv => tv.TreningId == id)
            .ToList();
        foreach (var tv in treningVjezbe)
        {
            tv.DeletedAt = DateTime.UtcNow;
        }
        
        _context.SaveChanges();
    }
}
```

---

## 2. CREATE OPERACIJA

### Princip

Create operacija prima podatke, validifera ih, i kreira novi zapis.

### Faze Create Operacije

1. **GET** - Prikaži formu za unos
2. **POST** - Prijedoh podatke i spremi u bazu
3. **Validacija** - Provjeri podatke prije spremanja

### Primjer - KorisnikController.Create()

```csharp
[HttpGet("novi", Name = "KorisnikCreate")]
public IActionResult Create()
{
    var model = CreateFormModel();
    PopulatePlanOptions(model);
    return View(model);
}

[HttpPost("novi")]
[ValidateAntiForgeryToken]
public IActionResult Create(KorisnikCreateViewModel model)
{
    // 1. VALIDACIJA
    if (model.RokCilja <= DateTime.Today)
    {
        ModelState.AddModelError(nameof(model.RokCilja), 
            "Rok cilja mora biti u budućnosti.");
    }

    if (model.DatumMjerenja < model.DatumRodjenja)
    {
        ModelState.AddModelError(nameof(model.DatumMjerenja), 
            "Datum mjerenja ne može biti prije datuma rođenja.");
    }

    // Validacija povezanog plana
    Plan? selectedPlan = null;
    if (model.PlanId.HasValue)
    {
        selectedPlan = _planRepository.GetById(model.PlanId.Value);
        if (selectedPlan is null)
        {
            ModelState.AddModelError(nameof(model.PlanId), 
                "Odabrani plan ne postoji.");
        }
    }

    if (!ModelState.IsValid)
    {
        PopulatePlanOptions(model);
        return View(model);
    }

    // 2. KREIRANJE ENTITETA
    var korisnik = new Korisnik
    {
        Ime = model.Ime.Trim(),
        Prezime = model.Prezime.Trim(),
        Email = model.Email.Trim(),
        DatumRodjenja = model.DatumRodjenja,
        DatumRegistracije = DateTime.Now,
        Visina = model.Visina,
        Tezina = model.Tezina,
        PlanId = model.PlanId,
        TrenutniPlan = selectedPlan
    };

    // 3. KREIRANJE POVEZANIH ZAPISA
    var cilj = new Cilj
    {
        Tip = model.TipCilja,
        CiljanaVrijednost = model.CiljanaVrijednost,
        Rok = model.RokCilja,
        Postignut = false,
        Korisnik = korisnik
    };

    var mjerenje = new Mjerenje
    {
        DatumMjerenja = model.DatumMjerenja,
        Tezina = model.MjerenjeTezina,
        PostotakMasti = model.PostotakMasti,
        OpsegStruka = model.OpsegStruka,
        Korisnik = korisnik
    };

    // 4. DODAVANJE U KOLEKCIJE
    korisnik.Ciljevi.Add(cilj);
    korisnik.Mjerenja.Add(mjerenje);
    selectedPlan?.Korisnici.Add(korisnik);

    // 5. SPREMANJE
    _korisnikRepository.Add(korisnik);

    // 6. PREUSMJERAVANJE
    return RedirectToRoute("KorisnikDetails", new 
    { 
        id = korisnik.Id, 
        slug = SlugHelper.Slugify($"{korisnik.Ime}-{korisnik.Prezime}") 
    });
}
```

### Primjer - TreningController.Create()

```csharp
[HttpGet("novi", Name = "TreningCreate")]
public IActionResult Create()
{
    var model = new TreningCreateViewModel
    {
        DatumVrijeme = DateTime.Now,
        AvailableKorisnici = _korisnikRepository.GetAll()
            .Select(k => new TreningKorisnikOption
            {
                Id = k.Id,
                FullName = $"{k.Ime} {k.Prezime}"
            })
            .OrderBy(k => k.FullName)
            .ToList()
    };

    return View(model);
}

[HttpPost("novi")]
[ValidateAntiForgeryToken]
public IActionResult Create(TreningCreateViewModel model)
{
    // Validacija korisnika
    var korisnik = _korisnikRepository.GetById(model.KorisnikId);
    if (korisnik is null)
    {
        ModelState.AddModelError(nameof(model.KorisnikId), 
            "Odabrani korisnik ne postoji.");
    }

    // Validacija datuma
    if (model.DatumVrijeme > DateTime.Now.AddDays(1))
    {
        ModelState.AddModelError(nameof(model.DatumVrijeme), 
            "Datum treninga ne smije biti previše u budućnosti.");
    }

    if (!ModelState.IsValid)
    {
        model.AvailableKorisnici = _korisnikRepository.GetAll()
            .Select(k => new TreningKorisnikOption
            {
                Id = k.Id,
                FullName = $"{k.Ime} {k.Prezime}"
            })
            .OrderBy(k => k.FullName)
            .ToList();
        return View(model);
    }

    var trening = new Trening
    {
        KorisnikId = model.KorisnikId,
        DatumVrijeme = model.DatumVrijeme,
        TrajanjeMinuta = model.TrajanjeMinuta,
        Vrsta = model.Vrsta,
        Biljeske = model.Biljeske?.Trim(),
        PotroseneKalorije = model.PotroseneKalorije,
        Ocjena = model.Ocjena,
        Korisnik = korisnik
    };

    _treningRepository.Add(trening);

    return RedirectToRoute("TreningDetails", new { id = trening.Id });
}
```

### Primjer - MjerenjeController.Create()

```csharp
[HttpPost("novo")]
[ValidateAntiForgeryToken]
public IActionResult Create(MjerenjeCreateViewModel model)
{
    // Validacija korisnika
    var korisnik = _korisnikRepository.GetById(model.KorisnikId);
    if (korisnik is null)
    {
        ModelState.AddModelError(nameof(model.KorisnikId), 
            "Odabrani korisnik ne postoji.");
    }

    // Validacija datuma - nakon datuma rođenja
    if (korisnik != null && model.DatumMjerenja < korisnik.DatumRodjenja)
    {
        ModelState.AddModelError(nameof(model.DatumMjerenja), 
            "Datum mjerenja ne smije biti prije datuma rođenja.");
    }

    // Validacija datuma - nije u budućnosti
    if (model.DatumMjerenja > DateTime.Today)
    {
        ModelState.AddModelError(nameof(model.DatumMjerenja), 
            "Datum mjerenja ne smije biti u budućnosti.");
    }

    if (!ModelState.IsValid)
    {
        model.AvailableKorisnici = _korisnikRepository.GetAll()
            .Select(k => new MjerenjeKorisnikOption
            {
                Id = k.Id,
                FullName = $"{k.Ime} {k.Prezime}"
            })
            .OrderBy(k => k.FullName)
            .ToList();
        return View(model);
    }

    var mjerenje = new Mjerenje
    {
        KorisnikId = model.KorisnikId,
        DatumMjerenja = model.DatumMjerenja,
        Tezina = model.Tezina,
        PostotakMasti = model.PostotakMasti,
        OpsegStruka = model.OpsegStruka,
        Korisnik = korisnik
    };

    _mjerenjeRepository.Add(mjerenje);

    return RedirectToRoute("MjerenjeDetails", new { id = mjerenje.Id });
}
```

### Best Practices za Create

1. **Trimanje stringova** - `model.Naziv.Trim()`
2. **Validacija relacija** - Provjera postoji li povezani entitet
3. **Datumske provjere** - Datumi ne smiju biti u budućnosti
4. **ModelState.IsValid** - Uvijek provjeri prije spremanja
5. **Trim bilješki** - `model.Biljeske?.Trim()`

---

## 3. KONTROLNE AKCIJE

### Dostupne akcije po kontroleru

#### KorisnikController
- ✅ Index - Pregled
- ✅ Details - Detalji
- ✅ Create - Kreiranje
- ✅ Edit - Ažuriranje
- ✅ Delete - Brisanje (soft)

#### TreningController
- ✅ Index - Pregled sa filterima
- ✅ Details - Detalji
- ✅ Create - Kreiranje
- ✅ Delete - Brisanje (soft)

#### CiljController
- ✅ Index - Pregled
- ✅ Details - Detalji
- ✅ Delete - Brisanje (soft)

#### VjezbaController
- ✅ Index - Pregled
- ✅ Details - Detalji
- ✅ Create - Kreiranje
- ✅ Edit - Ažuriranje
- ✅ Delete - Brisanje (soft)

#### PlanController
- ✅ Index - Pregled
- ✅ Details - Detalji
- ✅ Create - Kreiranje
- ✅ Delete - Brisanje (soft)

#### MjerenjeController
- ✅ Index - Pregled sa filterima
- ✅ Details - Detalji sa analitikom
- ✅ Create - Kreiranje
- ✅ Delete - Brisanje (soft)

#### TreningVjezbaController
- ✅ Index - Pregled
- ✅ Details - Detalji
- ✅ Delete - Brisanje (soft)

---

## 4. VALIDACIJA

### Data Annotations

```csharp
[Required(ErrorMessage = "Naziv je obavezan")]
[StringLength(100, MinimumLength = 3)]
[Range(1, 100)]
[EmailAddress]
[DataType(DataType.Date)]
[Display(Name = "Naziv")]
public string Naziv { get; set; }
```

### Primjer Custom Validacije

```csharp
if (model.RokCilja <= DateTime.Today)
{
    ModelState.AddModelError(nameof(model.RokCilja), 
        "Rok cilja mora biti u budućnosti.");
}

if (!ModelState.IsValid)
{
    return View(model);
}
```

### Anti-Forgery Token

```html
<form method="post">
    @Html.AntiForgeryToken()
    <!-- Forma -->
</form>
```

```csharp
[ValidateAntiForgeryToken]
public IActionResult Create(ViewModel model)
{
    // ...
}
```

---

## 5. MIGRACIJE

### Kreiraj migraciju za DeletedAt

```bash
dotnet ef migrations add AddSoftDeleteFields
```

### Primijeni migraciju

```bash
dotnet ef database update
```

### Generirani kod

Migracija dodaje `DeletedAt` kolone za sve modele:

```sql
ALTER TABLE [Korisnici] ADD [DeletedAt] datetime2 NULL;
ALTER TABLE [Ciljevi] ADD [DeletedAt] datetime2 NULL;
ALTER TABLE [Treninzi] ADD [DeletedAt] datetime2 NULL;
-- ... ostali modeli
```

---

## 6. PRIMJENA

### Soft Delete u praksi

**Prije:** Korisnik Marko se brišu iz baze
```sql
DELETE FROM Korisnici WHERE Id = 1;
```

**Sada:** Korisnik Marko se markirah kao obrisan
```sql
UPDATE Korisnici SET DeletedAt = '2026-05-10 10:26:13' WHERE Id = 1;
```

### Automatski filter

Svi upiti automatski filtriraju:
```csharp
var korisnici = _korisnikRepository.GetAll();
// SQL: SELECT * FROM Korisnici WHERE DeletedAt IS NULL
```

### Vidljive podatke

- Aktivni korisnici: 3
- Obrisani korisnici (u bazi): 1
- Ukupno: 4

---

## 7. ZAKLJUČAK

Ova implementacija pruža:
- ✅ Fleksibilnost - Mogu se obrisani podaci vratiti
- ✅ Bezbijednost - Nema gubitka podataka
- ✅ Auditna trka - Kada je što obrisano
- ✅ Jednostavnost - Automatski filtri
- ✅ Skalabilnost - Lako proširiti na sve modele
