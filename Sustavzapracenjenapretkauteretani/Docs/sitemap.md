# Sitemap — Mapa URL ruta

> **Projekt:** Sustav za praćenje napretka u teretani  
> **Routing:** Attribute routing (`app.MapControllers()`)  
> **Konvencija naziva:** `{KontrolerAkcija}` (npr. `KorisnikDetails`, `VjezbeIndex`)  
> **Slug helper:** `SlugHelper.Slugify()` — generira URL-friendly tekst iz naziva

---

## Sadržaj

1. [Home Controller](#1-home-controller)
2. [Korisnik Controller](#2-korisnik-controller)
3. [Vjezba Controller](#3-vjezba-controller)
4. [Trening Controller](#4-trening-controller)
5. [TreningVjezba Controller](#5-treningvjezba-controller)
6. [Plan Controller](#6-plan-controller)
7. [Cilj Controller](#7-cilj-controller)
8. [Mjerenje Controller](#8-mjerenje-controller)
9. [Sažetak svih ruta](#9-sažetak-svih-ruta)
10. [Navigacijske putanje](#10-navigacijske-putanje)

---

## 1. Home Controller

**Prefiks:** `[Route("")]`  
**Controller:** `Web/Controllers/HomeController.cs`

| HTTP | URL | Naziv rute | Akcija | View | Opis |
|------|-----|-----------|--------|------|------|
| GET | `/` | `HomeRoot` | `Index` | `Home/Index.cshtml` | Početna stranica — dashboard |
| GET | `/pocetna` | `HomePocetna` | `Index` | `Home/Index.cshtml` | Alias za početnu stranicu |
| GET | `/privatnost` | `HomePrivacy` | `Privacy` | `Home/Privacy.cshtml` | Politika privatnosti |
| GET | `/greska` | `HomeError` | `Error` | `Home/Error.cshtml` | Stranica za greške |

> Dvije rute (`HomeRoot` i `HomePocetna`) vode na istu akciju `Index`.

---

## 2. Korisnik Controller

**Prefiks:** `[Route("korisnici")]`  
**Controller:** `Web/Controllers/KorisnikController.cs`  
**Model:** `Korisnik` / `KorisnikCreateViewModel`

| HTTP | URL pattern | Primjer URL-a | Naziv rute | Akcija | View | Opis |
|------|------------|--------------|-----------|--------|------|------|
| GET | `/korisnici` | `/korisnici` | `KorisniciIndex` | `Index` | `Korisnik/Index.cshtml` | Lista svih korisnika |
| GET | `/korisnici/{id}/{slug?}` | `/korisnici/5/ana-horvat` | `KorisnikDetails` | `Details` | `Korisnik/Details.cshtml` | Detalji korisnika |
| GET | `/korisnici/novi` | `/korisnici/novi` | `KorisnikCreate` | `Create` | `Korisnik/Create.cshtml` | Forma za novog korisnika |
| POST | `/korisnici/novi` | `/korisnici/novi` | *(nema imena)* | `Create` | `Korisnik/Create.cshtml` | Pohrana novog korisnika |
| GET | `/korisnici/{id}/{slug?}/uredi` | `/korisnici/5/ana-horvat/uredi` | `KorisnikEdit` | `Edit` | `Korisnik/Edit.cshtml` | Forma za uređivanje |
| POST | `/korisnici/{id}/{slug?}/uredi` | `/korisnici/5/ana-horvat/uredi` | `KorisnikEditPost` | `Edit` | `Korisnik/Edit.cshtml` | Pohrana izmjena |
| GET | `/korisnici/{id}/{slug?}/obrisi` | `/korisnici/5/ana-horvat/obrisi` | `KorisnikDelete` | `Delete` | `Korisnik/Delete.cshtml` | Potvrda brisanja |
| POST | `/korisnici/{id}/{slug?}/obrisi` | `/korisnici/5/ana-horvat/obrisi` | `KorisnikDeletePost` | `DeleteConfirmed` | *(redirect)* | Izvršavanje brisanja |

**Route parametri:**
- `{id:int}` — obavezan, ID korisnika
- `{slug?}` — opcionalan, npr. `ana-horvat` (generira `SlugHelper.Slugify(Ime + "-" + Prezime)`)

---

## 3. Vjezba Controller

**Prefiks:** `[Route("vjezbe")]`  
**Controller:** `Web/Controllers/VjezbaController.cs`  
**Model:** `Vjezba` / `VjezbaCreateViewModel`

| HTTP | URL pattern | Primjer URL-a | Naziv rute | Akcija | View | Opis |
|------|------------|--------------|-----------|--------|------|------|
| GET | `/vjezbe` | `/vjezbe` | `VjezbeIndex` | `Index` | `Vjezba/Index.cshtml` | Katalog vježbi |
| GET | `/vjezbe/{id}/{slug?}` | `/vjezbe/3/bench-press` | `VjezbaDetails` | `Details` | `Vjezba/Details.cshtml` | Detalji vježbe |
| GET | `/vjezbe/nova` | `/vjezbe/nova` | `VjezbaCreate` | `Create` | `Vjezba/Create.cshtml` | Forma za novu vježbu |
| POST | `/vjezbe/nova` | `/vjezbe/nova` | *(nema imena)* | `Create` | `Vjezba/Create.cshtml` | Pohrana nove vježbe |
| GET | `/vjezbe/{id}/{slug?}/uredi` | `/vjezbe/3/bench-press/uredi` | `VjezbaEdit` | `Edit` | `Vjezba/Edit.cshtml` | Forma za uređivanje |
| POST | `/vjezbe/{id}/{slug?}/uredi` | `/vjezbe/3/bench-press/uredi` | `VjezbaEditPost` | `Edit` | `Vjezba/Edit.cshtml` | Pohrana izmjena |
| GET | `/vjezbe/{id}/{slug?}/obrisi` | `/vjezbe/3/bench-press/obrisi` | `VjezbaDelete` | `Delete` | `Vjezba/Delete.cshtml` | Potvrda brisanja |
| POST | `/vjezbe/{id}/{slug?}/obrisi` | `/vjezbe/3/bench-press/obrisi` | `VjezbaDeletePost` | `DeleteConfirmed` | *(redirect)* | Izvršavanje brisanja |

**Route parametri:**
- `{id:int}` — obavezan, ID vježbe
- `{slug?}` — opcionalan, npr. `bench-press` (generira `SlugHelper.Slugify(Naziv)`)

---

## 4. Trening Controller

**Prefiks:** `[Route("treninzi")]`  
**Controller:** `Web/Controllers/TreningController.cs`  
**Model:** `Trening`

| HTTP | URL pattern | Primjer URL-a | Naziv rute | Akcija | View | Opis |
|------|------------|--------------|-----------|--------|------|------|
| GET | `/treninzi` | `/treninzi` | `TreninziIndex` | `Index` | `Trening/Index.cshtml` | Lista treninga s filterima |
| GET | `/treninzi/{id}/{slug?}` | `/treninzi/10/trening-snage` | `TreningDetails` | `Details` | `Trening/Details.cshtml` | Detalji treninga |
| GET | `/treninzi/{id}/{slug?}/obrisi` | `/treninzi/10/trening-snage/obrisi` | `TreningDelete` | `Delete` | `Trening/Delete.cshtml` | Potvrda brisanja |
| POST | `/treninzi/{id}/{slug?}/obrisi` | `/treninzi/10/trening-snage/obrisi` | `TreningDeletePost` | `DeleteConfirmed` | *(redirect)* | Izvršavanje brisanja |

**Query string parametri (Index akcija):**

| Parametar | Tip | Default | Opis |
|-----------|-----|---------|------|
| `userId` | `int?` | `null` | Filter po korisniku |
| `vrsta` | `VrstaTreninga?` | `null` | Filter po vrsti treninga |
| `period` | `int` | `0` | Vremenski period filtriranja |
| `sort` | `string` | `"date_desc"` | Redoslijed sortiranja |

Primjer: `/treninzi?userId=3&vrsta=1&sort=date_asc`

---

## 5. TreningVjezba Controller

**Prefiks:** `[Route("trening-vjezbe")]`  
**Controller:** `Web/Controllers/TreningVjezbaController.cs`  
**Model:** `TreningVjezba`

| HTTP | URL pattern | Primjer URL-a | Naziv rute | Akcija | View | Opis |
|------|------------|--------------|-----------|--------|------|------|
| GET | `/trening-vjezbe` | `/trening-vjezbe` | `TreningVjezbeIndex` | `Index` | `TreningVjezba/Index.cshtml` | Lista vježbi u treninzima |
| GET | `/trening-vjezbe/{id}/{slug?}` | `/trening-vjezbe/5/bench-press` | `TreningVjezbaDetails` | `Details` | `TreningVjezba/Details.cshtml` | Detalji vježbe u treningu |
| GET | `/trening-vjezbe/{id}/{slug?}/obrisi` | `/trening-vjezbe/5/bench-press/obrisi` | `TreningVjezbaDelete` | `Delete` | `TreningVjezba/Delete.cshtml` | Potvrda brisanja |
| POST | `/trening-vjezbe/{id}/{slug?}/obrisi` | `/trening-vjezbe/5/bench-press/obrisi` | `TreningVjezbaDeletePost` | `DeleteConfirmed` | *(redirect)* | Izvršavanje brisanja |

---

## 6. Plan Controller

**Prefiks:** `[Route("planovi")]`  
**Controller:** `Web/Controllers/PlanController.cs`  
**Model:** `Plan`

| HTTP | URL pattern | Primjer URL-a | Naziv rute | Akcija | View | Opis |
|------|------------|--------------|-----------|--------|------|------|
| GET | `/planovi` | `/planovi` | `PlanoviIndex` | `Index` | `Plan/Index.cshtml` | Lista planova |
| GET | `/planovi/{id}/{slug?}` | `/planovi/1/pocetni-plan` | `PlanDetails` | `Details` | `Plan/Details.cshtml` | Detalji plana |
| GET | `/planovi/{id}/{slug?}/obrisi` | `/planovi/1/pocetni-plan/obrisi` | `PlanDelete` | `Delete` | `Plan/Delete.cshtml` | Potvrda brisanja |
| POST | `/planovi/{id}/{slug?}/obrisi` | `/planovi/1/pocetni-plan/obrisi` | `PlanDeletePost` | `DeleteConfirmed` | *(redirect)* | Izvršavanje brisanja |

---

## 7. Cilj Controller

**Prefiks:** `[Route("ciljevi")]`  
**Controller:** `Web/Controllers/CiljController.cs`  
**Model:** `Cilj`

| HTTP | URL pattern | Primjer URL-a | Naziv rute | Akcija | View | Opis |
|------|------------|--------------|-----------|--------|------|------|
| GET | `/ciljevi` | `/ciljevi` | `CiljeviIndex` | `Index` | `Cilj/Index.cshtml` | Lista ciljeva |
| GET | `/ciljevi/{id}/{slug?}` | `/ciljevi/2/mrsavljenje` | `CiljDetails` | `Details` | `Cilj/Details.cshtml` | Detalji cilja |
| GET | `/ciljevi/{id}/{slug?}/obrisi` | `/ciljevi/2/mrsavljenje/obrisi` | `CiljDelete` | `Delete` | `Cilj/Delete.cshtml` | Potvrda brisanja |
| POST | `/ciljevi/{id}/{slug?}/obrisi` | `/ciljevi/2/mrsavljenje/obrisi` | `CiljDeletePost` | `DeleteConfirmed` | *(redirect)* | Izvršavanje brisanja |

---

## 8. Mjerenje Controller

**Prefiks:** `[Route("mjerenja")]`  
**Controller:** `Web/Controllers/MjerenjeController.cs`  
**Model:** `Mjerenje`

| HTTP | URL pattern | Primjer URL-a | Naziv rute | Akcija | View | Opis |
|------|------------|--------------|-----------|--------|------|------|
| GET | `/mjerenja` | `/mjerenja` | `MjerenjaIndex` | `Index` | `Mjerenje/Index.cshtml` | Lista mjerenja s filterima |
| GET | `/mjerenja/{id}/{slug?}` | `/mjerenja/15/mjerenje-2026` | `MjerenjeDetails` | `Details` | `Mjerenje/Details.cshtml` | Detalji mjerenja |
| GET | `/mjerenja/{id}/{slug?}/obrisi` | `/mjerenja/15/mjerenje-2026/obrisi` | `MjerenjeDelete` | `Delete` | `Mjerenje/Delete.cshtml` | Potvrda brisanja |
| POST | `/mjerenja/{id}/{slug?}/obrisi` | `/mjerenja/15/mjerenje-2026/obrisi` | `MjerenjeDeletePost` | `DeleteConfirmed` | *(redirect)* | Izvršavanje brisanja |

**Query string parametri (Index akcija):**

| Parametar | Tip | Default | Opis |
|-----------|-----|---------|------|
| `userId` | `int?` | `null` | Filter po korisniku |
| `sortBy` | `string` | `"datumDesc"` | Redoslijed sortiranja |

Primjer: `/mjerenja?userId=3&sortBy=datumAsc`

---

## 9. Sažetak svih ruta

### Statistika

| Metrika | Vrijednost |
|---------|-----------|
| Controllera | 8 |
| Ukupno ruta | 32 |
| GET ruta | 25 |
| POST ruta | 7 |
| Ruta s punim CRUD-om | 2 (Korisnik, Vjezba) |
| Ruta samo s Details + Delete | 5 (Trening, TreningVjezba, Plan, Cilj, Mjerenje) |

### Kompletna tablica ruta

| Naziv rute | HTTP | URL |
|-----------|------|-----|
| `HomeRoot` | GET | `/` |
| `HomePocetna` | GET | `/pocetna` |
| `HomePrivacy` | GET | `/privatnost` |
| `HomeError` | GET | `/greska` |
| `KorisniciIndex` | GET | `/korisnici` |
| `KorisnikDetails` | GET | `/korisnici/{id}/{slug?}` |
| `KorisnikCreate` | GET | `/korisnici/novi` |
| *(nema)* | POST | `/korisnici/novi` |
| `KorisnikEdit` | GET | `/korisnici/{id}/{slug?}/uredi` |
| `KorisnikEditPost` | POST | `/korisnici/{id}/{slug?}/uredi` |
| `KorisnikDelete` | GET | `/korisnici/{id}/{slug?}/obrisi` |
| `KorisnikDeletePost` | POST | `/korisnici/{id}/{slug?}/obrisi` |
| `VjezbeIndex` | GET | `/vjezbe` |
| `VjezbaDetails` | GET | `/vjezbe/{id}/{slug?}` |
| `VjezbaCreate` | GET | `/vjezbe/nova` |
| *(nema)* | POST | `/vjezbe/nova` |
| `VjezbaEdit` | GET | `/vjezbe/{id}/{slug?}/uredi` |
| `VjezbaEditPost` | POST | `/vjezbe/{id}/{slug?}/uredi` |
| `VjezbaDelete` | GET | `/vjezbe/{id}/{slug?}/obrisi` |
| `VjezbaDeletePost` | POST | `/vjezbe/{id}/{slug?}/obrisi` |
| `TreninziIndex` | GET | `/treninzi` |
| `TreningDetails` | GET | `/treninzi/{id}/{slug?}` |
| `TreningDelete` | GET | `/treninzi/{id}/{slug?}/obrisi` |
| `TreningDeletePost` | POST | `/treninzi/{id}/{slug?}/obrisi` |
| `TreningVjezbeIndex` | GET | `/trening-vjezbe` |
| `TreningVjezbaDetails` | GET | `/trening-vjezbe/{id}/{slug?}` |
| `TreningVjezbaDelete` | GET | `/trening-vjezbe/{id}/{slug?}/obrisi` |
| `TreningVjezbaDeletePost` | POST | `/trening-vjezbe/{id}/{slug?}/obrisi` |
| `PlanoviIndex` | GET | `/planovi` |
| `PlanDetails` | GET | `/planovi/{id}/{slug?}` |
| `PlanDelete` | GET | `/planovi/{id}/{slug?}/obrisi` |
| `PlanDeletePost` | POST | `/planovi/{id}/{slug?}/obrisi` |
| `CiljeviIndex` | GET | `/ciljevi` |
| `CiljDetails` | GET | `/ciljevi/{id}/{slug?}` |
| `CiljDelete` | GET | `/ciljevi/{id}/{slug?}/obrisi` |
| `CiljDeletePost` | POST | `/ciljevi/{id}/{slug?}/obrisi` |
| `MjerenjaIndex` | GET | `/mjerenja` |
| `MjerenjeDetails` | GET | `/mjerenja/{id}/{slug?}` |
| `MjerenjeDelete` | GET | `/mjerenja/{id}/{slug?}/obrisi` |
| `MjerenjeDeletePost` | POST | `/mjerenja/{id}/{slug?}/obrisi` |

### Obrazac ruta po tipu akcije

```
Index    →  GET  /{prefiks}
Details  →  GET  /{prefiks}/{id}/{slug?}
Create   →  GET  /{prefiks}/novi|nova        (samo Korisnik, Vjezba)
Create   →  POST /{prefiks}/novi|nova        (samo Korisnik, Vjezba)
Edit     →  GET  /{prefiks}/{id}/{slug?}/uredi   (samo Korisnik, Vjezba)
Edit     →  POST /{prefiks}/{id}/{slug?}/uredi   (samo Korisnik, Vjezba)
Delete   →  GET  /{prefiks}/{id}/{slug?}/obrisi
Delete   →  POST /{prefiks}/{id}/{slug?}/obrisi
```

---

## 10. Navigacijske putanje

### Kreiranje korisnika

```
GET /                          (HomeRoot — početna)
  → GET /korisnici             (KorisniciIndex — lista)
  → GET /korisnici/novi        (KorisnikCreate — forma)
  → POST /korisnici/novi       (pohrana)
  → GET /korisnici/5/ana-horvat (KorisnikDetails — detalji)
```

### Dodavanje vježbe i uređivanje

```
GET /vjezbe                              (VjezbeIndex — katalog)
  → GET /vjezbe/nova                    (VjezbaCreate — forma)
  → POST /vjezbe/nova                   (pohrana)
  → GET /vjezbe/3/bench-press           (VjezbaDetails)
  → GET /vjezbe/3/bench-press/uredi     (VjezbaEdit — forma)
  → POST /vjezbe/3/bench-press/uredi    (pohrana izmjena)
  → GET /vjezbe/3/bench-press           (VjezbaDetails)
```

### Brisanje zapisa (svaki entitet)

```
GET /{prefiks}/{id}/{slug?}          (Details — detalji)
  → GET /{prefiks}/{id}/{slug?}/obrisi  (Delete — potvrda)
  → POST /{prefiks}/{id}/{slug?}/obrisi (DeletePost — brisanje)
  → GET /{prefiks}                      (Index — redirect natrag)
```

### Pregled treninga s filterom

```
GET /treninzi                             (svi treninzi)
GET /treninzi?userId=3                    (treninzi korisnika 3)
GET /treninzi?vrsta=1&sort=date_asc       (samo Snaga treninzi, stariji prvi)
GET /treninzi/10/trening-snage            (detalji jednog treninga)
```
