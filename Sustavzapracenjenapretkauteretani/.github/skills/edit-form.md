# Skill: Izrada Edit/Create forme

Koristi ovaj skill svaki put kad trebaš dodati Create ili Edit stranicu za neki entitet u projektu.

---

## Kada koristiti

- Kad entitet ima samo `Index` i `Details`, a treba mu i `Create` i/ili `Edit`
- Kad trebaš obrazac za unos ili izmjenu podataka s validacijom

---

## Struktura projekta (bitni folderi)

```
Models/                         ← ViewModels za forme idu ovdje
DAL/Repositories/               ← Repozitoriji s Add(), Update(), Delete()
Controllers/                    ← Controller s GET+POST akcijama
Views/{NazivControllera}/       ← Create.cshtml, Edit.cshtml, _FormPartial.cshtml
```

---

## Korak 1 — ViewModel (Models/XyzCreateViewModel.cs)

ViewModel sadrži SAMO polja koja korisnik unosi u formu. Nije isti kao domenski model.

```csharp
using System.ComponentModel.DataAnnotations;

namespace Sustavzapracenjenapretkauteretani.Models;

public class XyzCreateViewModel
{
    public int? Id { get; set; }  // null = create, broj = edit

    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(100, ErrorMessage = "Naziv ne smije biti duži od 100 znakova.")]
    public string Naziv { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Opis { get; set; }

    // Za enum polja koristiti tip enuma:
    public NekiEnum TipPolja { get; set; }

    // Za opcionalne numeričke vrijednosti:
    [Range(1, 5)]
    [Display(Name = "Razina težine")]
    public int RazinaTezine { get; set; }

    // Za boolean:
    [Display(Name = "Potrebna oprema")]
    public bool PotrebnaOprema { get; set; }
}
```

**Pravila:**
- Svako polje treba `[Required]` ako je obavezno
- Koristiti `[Display(Name = "...")]` za hrvatski label u formi
- Koristiti `[Range]`, `[StringLength]`, `[EmailAddress]` prema tipu podatka
- `Id` je uvijek `int?` — koristi se da isti ViewModel radi i za Create i za Edit

---

## Korak 2 — Controller akcije

Dodati u postojeći controller **4 akcije**: GET Create, POST Create, GET Edit, POST Edit.

```csharp
// GET /xyz/nova — prikazuje praznu formu
[HttpGet("nova", Name = "XyzCreate")]
public IActionResult Create()
{
    return View(new XyzCreateViewModel());
}

// POST /xyz/nova — prima popunjenu formu i sprema
[HttpPost("nova")]
[ValidateAntiForgeryToken]
public IActionResult Create(XyzCreateViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    var entitet = new Xyz
    {
        Naziv = model.Naziv.Trim(),
        Opis = model.Opis?.Trim(),
        // ... ostala polja
    };

    _xyzRepository.Add(entitet);
    return RedirectToAction(nameof(Details), new { id = entitet.Id });
}

// GET /xyz/{id}/{slug?}/uredi — prikazuje formu s postojećim podacima
[HttpGet("{id:int}/{slug?}/uredi", Name = "XyzEdit")]
public IActionResult Edit(int id)
{
    var entitet = _xyzRepository.GetById(id);
    if (entitet is null) return NotFound();

    var model = new XyzCreateViewModel
    {
        Id = entitet.Id,
        Naziv = entitet.Naziv,
        Opis = entitet.Opis,
        // ... ostala polja
    };

    return View(model);
}

// POST /xyz/{id}/{slug?}/uredi — prima izmijenjenu formu i sprema
[HttpPost("{id:int}/{slug?}/uredi", Name = "XyzEditPost")]
[ValidateAntiForgeryToken]
public IActionResult Edit(int id, XyzCreateViewModel model)
{
    var entitet = _xyzRepository.GetById(id);
    if (entitet is null) return NotFound();

    if (!ModelState.IsValid)
    {
        model.Id = id;
        return View(model);
    }

    entitet.Naziv = model.Naziv.Trim();
    entitet.Opis = model.Opis?.Trim();
    // ... ostala polja

    _xyzRepository.Update();
    return RedirectToAction(nameof(Details), new { id = entitet.Id });
}
```

**Pravila:**
- GET akcija = prikazuje formu (vraća `View(model)`)
- POST akcija = prima formu, validira, sprema, preusmjerava
- Uvijek `[ValidateAntiForgeryToken]` na POST akcijama
- Uvijek provjeriti `ModelState.IsValid` — ako nije validan, vrati view s modelom
- Nakon uspješnog POST-a uvijek `RedirectToAction` (nikad `return View()`)
- Ruta za Edit POST mora imati i `{id:int}` jer je id potreban za dohvat

---

## Korak 3 — Shared Form Partial (_XyzFormPartial.cshtml)

Partial view sadrži samo polja forme — koristi se i u Create i u Edit.

```html
@model Sustavzapracenjenapretkauteretani.Models.XyzCreateViewModel

<div asp-validation-summary="ModelOnly" class="validation-summary"></div>

<section class="form-section">
    <h2>Naziv sekcije</h2>
    <div class="form-grid">

        <div class="form-field">
            <label asp-for="Naziv"></label>
            <input asp-for="Naziv" />
            <span asp-validation-for="Naziv" class="validation-text"></span>
        </div>

        <div class="form-field form-field-full">
            <label asp-for="Opis"></label>
            <textarea asp-for="Opis" rows="4"></textarea>
            <span asp-validation-for="Opis" class="validation-text"></span>
        </div>

        <!-- Za enum dropdown: -->
        <div class="form-field">
            <label asp-for="TipPolja"></label>
            <select asp-for="TipPolja" asp-items="Html.GetEnumSelectList<NekiEnum>()"></select>
            <span asp-validation-for="TipPolja" class="validation-text"></span>
        </div>

        <!-- Za checkbox: -->
        <div class="form-field">
            <label>
                <input asp-for="PotrebnaOprema" type="checkbox" />
                @Html.DisplayNameFor(m => m.PotrebnaOprema)
            </label>
        </div>

    </div>
</section>
```

**CSS klase dostupne u projektu (već definirane u site.css):**
- `form-section` — kartica sekcije forme
- `form-grid` — 2-stupčana grid mreža za polja
- `form-field` — wrapper jednog polja (label + input + validacija)
- `form-field-full` — polje koje zauzima cijelu širinu (grid-column: 1 / -1)
- `validation-summary` — crveni box s greškama na vrhu
- `validation-text` — crveni tekst ispod polja
- `btn-main` — zeleni primarni gumb
- `btn-muted` — sivi sekundarni gumb (Odustani)

---

## Korak 4 — Create.cshtml

```html
@model Sustavzapracenjenapretkauteretani.Models.XyzCreateViewModel

@{
    ViewData["Title"] = "Nova X";
}

<div class="create-wrapper">
    <div class="create-header">
        <h1>Nova X</h1>
        <p>Opis što korisnik ovdje unosi.</p>
    </div>

    <form asp-route="XyzCreate" method="post" class="create-form">
        @Html.AntiForgeryToken()
        <partial name="_XyzFormPartial" model="Model" />
        <div class="form-actions">
            <a asp-route="XyzIndex" class="btn-muted">Odustani</a>
            <button type="submit" class="btn-main">Spremi</button>
        </div>
    </form>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

---

## Korak 5 — Edit.cshtml

```html
@model Sustavzapracenjenapretkauteretani.Models.XyzCreateViewModel

@{
    ViewData["Title"] = "Uredi X";
}

<div class="create-wrapper">
    <div class="create-header">
        <h1>Uredi X</h1>
        <p>Ažurirajte podatke.</p>
    </div>

    <form asp-route="XyzEditPost"
          asp-route-id="@Model.Id"
          asp-route-slug="@SlugHelper.Slugify(Model.Naziv)"
          method="post" class="create-form">
        @Html.AntiForgeryToken()
        <partial name="_XyzFormPartial" model="Model" />
        <div class="form-actions">
            <a asp-route="XyzIndex" class="btn-muted">Odustani</a>
            <button type="submit" class="btn-main">Spremi promjene</button>
        </div>
    </form>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

**Razlika između Create i Edit forma:**
- `asp-route` u formi: Create koristi ime Create rute, Edit koristi EditPost rutu
- Edit forma ima dodatno `asp-route-id` i `asp-route-slug`
- Naslov: "Nova X" vs "Uredi X"
- Gumb: "Spremi" vs "Spremi promjene"

---

## Korak 6 — Dodati gumbe u Index i Details

**U Index.cshtml** dodati link za kreiranje:
```html
<a asp-route="XyzCreate" class="btn-main">
    <i class="fas fa-plus"></i> Nova X
</a>
```

**U Details.cshtml** dodati link za uređivanje:
```html
<a asp-route="XyzEdit"
   asp-route-id="@Model.Id"
   asp-route-slug="@SlugHelper.Slugify(Model.Naziv)"
   class="btn-muted">
    <i class="fas fa-edit"></i> Uredi
</a>
```

---

## Checklista za implementaciju

- [ ] Kreiran `XyzCreateViewModel.cs` u `Models/`
- [ ] Dodane 4 akcije u Controller (GET Create, POST Create, GET Edit, POST Edit)
- [ ] Kreiran `_XyzFormPartial.cshtml` u `Views/Xyz/`
- [ ] Kreiran `Create.cshtml` u `Views/Xyz/`
- [ ] Kreiran `Edit.cshtml` u `Views/Xyz/`
- [ ] Dodan gumb "Nova X" u `Index.cshtml`
- [ ] Dodan gumb "Uredi" u `Details.cshtml`

---

## Primjer implementacije: VjezbaController

Referentni primjer ovog patterna primijenjen na Vjezbe:
- ViewModel: `Models/VjezbaCreateViewModel.cs`
- Controller akcije: `Controllers/VjezbaController.cs` (Create + Edit)
- Partial: `Views/Vjezba/_VjezbaFormPartial.cshtml`
- Forme: `Views/Vjezba/Create.cshtml`, `Views/Vjezba/Edit.cshtml`
