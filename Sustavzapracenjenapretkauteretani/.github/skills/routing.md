# Skill: Routing — dodavanje i konfiguracija URL ruta

Koristi ovaj skill svaki put kad trebaš dodati novu rutu, promijeniti URL strukturu ili razumjeti kako routing funkcionira u projektu.

---

## Kada koristiti

- Dodavanje nove akcije u postojeći controller
- Kreiranje potpuno novog controllera s rutama
- Promjena URL sheme postojeće rute
- Dodavanje route constraintova ili query parametara
- Debugiranje zašto određeni URL ne pronalazi akciju

---

## Struktura projekta (relevantni folderi)

```
Controllers/                  ← controlleri s [Route] i [HttpGet/Post] atributima
Views/{NazivControllera}/     ← view datoteke (naziv mora odgovarati controlleru)
Program.cs                    ← fallback ruta (ne koristi se aktivno)
```

---

## Osnovno pravilo kombiniranja ruta

```
Finalni URL = [Route na klasi] + [Route na akciji]
```

Primjer:

```csharp
[Route("vjezbe")]                          // prefiks klase
public class VjezbaController : Controller
{
    [HttpGet("nova")]                      // + "nova"
    public IActionResult Create() { }      // → GET /vjezbe/nova

    [HttpGet("{id:int}/{slug?}")]          // + "{id:int}/{slug?}"
    public IActionResult Details(int id) { } // → GET /vjezbe/5 ili /vjezbe/5/bench-press
}
```

---

## Korak 1 — Novi controller

Svaki novi controller mora slijediti sljedeći obrazac:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Sustavzapracenjenapretkauteretani.Controllers;

[Route("moj-prefiks")]                     // jedinstven prefiks, malim slovima, crtice umjesto razmaka
public class MojController : Controller
{
    // konstruktor s injektiranim repozitorijem
    private readonly MojRepository _mojRepository;

    public MojController(MojRepository mojRepository)
    {
        _mojRepository = mojRepository;
    }
}
```

**Konvencija prefiksa:**

| Controller | Prefiks |
|------------|---------|
| KorisnikController | `korisnici` |
| VjezbaController | `vjezbe` |
| TreningController | `treninzi` |
| TreningVjezbaController | `trening-vjezbe` |
| NoviController | `novi-entiteti` |

---

## Korak 2 — Standardne akcije

### Index (lista svih)

```csharp
[HttpGet("", Name = "MojiIndex")]
public IActionResult Index()
{
    return View(_mojRepository.GetAll());
}
```

URL: `GET /moj-prefiks`

---

### Details (detalji jednog)

```csharp
[HttpGet("{id:int}/{slug?}", Name = "MojDetails")]
public IActionResult Details(int id)
{
    var entitet = _mojRepository.GetById(id);
    if (entitet is null)
        return NotFound();

    return View(entitet);
}
```

URL: `GET /moj-prefiks/5` ili `GET /moj-prefiks/5/naziv-entiteta`

---

### Create (forma + spremi)

```csharp
[HttpGet("novi", Name = "MojCreate")]
public IActionResult Create()
{
    return View(new MojCreateViewModel());
}

[HttpPost("novi")]
[ValidateAntiForgeryToken]
public IActionResult Create(MojCreateViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    // spremi i preusmjeri
    return RedirectToAction(nameof(Details), new { id = entitet.Id });
}
```

URL: `GET /moj-prefiks/novi` i `POST /moj-prefiks/novi`

---

### Edit (forma + spremi)

```csharp
[HttpGet("{id:int}/{slug?}/uredi", Name = "MojEdit")]
public IActionResult Edit(int id)
{
    var entitet = _mojRepository.GetById(id);
    if (entitet is null)
        return NotFound();

    return View(/* model */);
}

[HttpPost("{id:int}/{slug?}/uredi", Name = "MojEditPost")]
[ValidateAntiForgeryToken]
public IActionResult Edit(int id, MojCreateViewModel model)
{
    var entitet = _mojRepository.GetById(id);
    if (entitet is null)
        return NotFound();

    if (!ModelState.IsValid)
    {
        model.Id = id;
        return View(model);
    }

    // spremi i preusmjeri
    _mojRepository.Update();
    return RedirectToAction(nameof(Details), new { id = entitet.Id });
}
```

URL: `GET /moj-prefiks/5/uredi` i `POST /moj-prefiks/5/uredi`

---

## Korak 3 — Route parametri

### Vrste parametara

| Vrsta | Sintaksa | Primjer URL-a | Opis |
|-------|----------|--------------|------|
| Obavezni int | `{id:int}` | `/vjezbe/5` | Mora biti cijeli broj |
| Opcionalni slug | `{slug?}` | `/vjezbe/5/bench-press` | Tekstualni, može izostati |
| Query string | — | `/treninzi?userId=3` | Iza upitnika, mapira se na parametar akcije |

### Route constraints (ograničenja)

```csharp
{id:int}            // samo cijeli broj
{slug:minlength(1)} // string minimalne duljine 1
{lang:length(2)}    // string točno 2 znaka
{page:min(1)}       // int minimalno 1
```

### Query string parametri

Ne definiraju se u route atributu — direktno se mapiraju kao parametri akcije:

```csharp
[HttpGet("", Name = "TreninziIndex")]
public IActionResult Index(int? userId, string sort = "date_desc", int period = 0)
//                          ↑ iz ?userId=3  ↑ iz ?sort=date_asc   ↑ iz ?period=30
```

URL: `/treninzi?userId=3&sort=date_asc&period=30`

---

## Korak 4 — Named routes

Svaka akcija treba `Name` u atributu — koristi se za generiranje URL-a u viewovima.

**Konvencija naziva:**

```
{NazivControllera (bez "Controller")}{NazivAkcije}
```

Primjeri:

| Controller | Akcija | Naziv rute |
|------------|--------|-----------|
| KorisnikController | Index | `KorisniciIndex` |
| KorisnikController | Details | `KorisnikDetails` |
| KorisnikController | Create (GET) | `KorisnikCreate` |
| KorisnikController | Edit (GET) | `KorisnikEdit` |
| KorisnikController | Edit (POST) | `KorisnikEditPost` |

**Korištenje u Razor viewu:**

```html
<!-- Bez parametara -->
<a asp-route="VjezbeIndex">Sve vježbe</a>

<!-- S parametrom id -->
<a asp-route="VjezbaDetails" asp-route-id="@vjezba.Id">Detalji</a>

<!-- S id-em i opcionanim slugom -->
<a asp-route="VjezbaEdit"
   asp-route-id="@vjezba.Id"
   asp-route-slug="@SlugHelper.Slugify(vjezba.Naziv)">
    Uredi
</a>

<!-- Forma s POST rutom -->
<form asp-route="VjezbaEditPost"
      asp-route-id="@Model.Id"
      asp-route-slug="@SlugHelper.Slugify(Model.Naziv)"
      method="post">
```

**Preusmjeravanje u controlleru:**

```csharp
// Na akciju istog controllera
return RedirectToAction(nameof(Details), new { id = entitet.Id });

// Na akciju drugog controllera
return RedirectToAction(nameof(Index), "Korisnik");
```

---

## Korak 5 — Registracija u Program.cs

Novi controlleri **ne trebaju** nikakvu registraciju u `Program.cs` — attribute routing ih automatski pronalazi.

Jedina ruta u `Program.cs` je fallback:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Ova ruta je neaktivna dok god controller ima vlastite `[Route]` atribute.

---

## Slug generiranje

Projekt koristi `SlugHelper.Slugify()` iz `Utilities/SlugHelper.cs`:

```csharp
// U controlleru — nije potrebno, slug se ignorira u logici
// U viewu — za generiranje čitljivog URL-a
@SlugHelper.Slugify(vjezba.Naziv)        // "Bench Press" → "bench-press"
@SlugHelper.Slugify($"{k.Ime} {k.Prezime}") // "Ivan Horvat" → "ivan-horvat"
```

---

## Checklista za novu rutu

- [ ] Controller ima `[Route("prefiks")]` na razini klase
- [ ] Akcija ima `[HttpGet]` ili `[HttpPost]` s patternom i `Name`
- [ ] Naziv rute slijedi konvenciju `{Controller}{Akcija}`
- [ ] Route constraint `{id:int}` dodan gdje je id obavezan
- [ ] Slug parametar označen kao opcionalan `{slug?}`
- [ ] View datoteka kreirana u `Views/{NazivControllera}/{NazivAkcije}.cshtml`
- [ ] POST akcija ima `[ValidateAntiForgeryToken]`
- [ ] Links u viewovima koriste `asp-route` umjesto hardkodiranih href stringova

---

## Česti problemi

| Problem | Uzrok | Rješenje |
|---------|-------|---------|
| 404 za ispravnu URL putanju | Pogrešan prefix u `[Route]` na klasi | Provjeri da li se URL iz atributa klase i akcije ispravno kombiniraju |
| Ambiguous routes | Dvije akcije s istim URL patternom i HTTP metodom | Promijeni pattern jedne akcije ili dodaj route constraint |
| `id` dolazi kao `null` | Nedostaje `{id:int}` u patternu | Dodaj parametar u `[HttpGet("{id:int}")]` |
| Link generira pogrešan URL | Korišten `href` umjesto `asp-route` | Zamijeni s `asp-route="NazivRute"` |
| View nije pronađen | Naziv datoteke ne odgovara nazivu akcije | View mora biti `Views/{Controller}/{NazivAkcije}.cshtml` |
