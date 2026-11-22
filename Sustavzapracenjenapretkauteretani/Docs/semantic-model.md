# Semantički model baze podataka — Teretana

> **Projekt:** Sustav za praćenje napretka u teretani  
> **Baza:** `Teretana` (SQL Server Express)  
> **ORM:** Entity Framework Core 8  
> **DbContext:** `TeretanaDbContext` (`DAL/TeretanaDbContext.cs`)

---

## Sadržaj

1. [Pregled tablica](#1-pregled-tablica)
2. [Enumeracije](#2-enumeracije)
3. [Detalji tablica](#3-detalji-tablica)
4. [Veze između entiteta](#4-veze-između-entiteta)
5. [ER dijagram](#5-er-dijagram)
6. [Kaskadno brisanje](#6-kaskadno-brisanje)
7. [Seed data](#7-seed-data)
8. [DbContext — DbSet svojstva](#8-dbcontext--dbset-svojstva)

---

## 1. Pregled tablica

| # | C# klasa | SQL tablica | Opis |
|---|----------|-------------|------|
| 1 | `Korisnik` | `Korisnici` | Registrirani članovi teretane |
| 2 | `Trening` | `Treninzi` | Pojedinačni treninzi korisnika |
| 3 | `Vjezba` | `Vjezbe` | Kataloška vježba (master podaci) |
| 4 | `TreningVjezba` | `TreningVjezbe` | Asocijativna tablica: vježba u treningu (N:N s detaljima) |
| 5 | `Plan` | `Planovi` | Preddefinirani planovi treninga |
| 6 | `Cilj` | `Ciljevi` | Fitness cilj korisnika |
| 7 | `Mjerenje` | `Mjerenja` | Periodička tjelesna mjerenja |
| – | *(auto)* | `PlanVjezba` | Join tablica N:N između `Planovi` i `Vjezbe` |

---

## 2. Enumeracije

Sve enumeracije pohranjuju se u bazu kao `INT`.

### `VrstaTreninga`
| Vrijednost | Int | Tablica koja koristi |
|-----------|-----|----------------------|
| `Snaga` | 1 | `Treninzi` |
| `Kardio` | 2 | `Treninzi` |
| `HIIT` | 3 | `Treninzi` |
| `Funkcionalni` | 4 | `Treninzi` |
| `Mobility` | 5 | `Treninzi` |

### `MisicnaSkupina`
| Vrijednost | Int | Boja (UI) |
|-----------|-----|-----------|
| `Prsa` | 1 | `#ef4444` |
| `Leda` | 2 | `#f97316` |
| `Ramena` | 3 | `#eab308` |
| `Biceps` | 4 | `#3b82f6` |
| `Triceps` | 5 | `#06b6d4` |
| `Noge` | 6 | `#8b5cf6` |
| `Trbuh` | 7 | `#14b8a6` |
| `Kardio` | 8 | `#ec4899` |
| `CijeloTijelo` | 9 | `#6366f1` |

### `RazinaIskustva`
| Vrijednost | Int |
|-----------|-----|
| `Pocetnik` | 1 |
| `Srednji` | 2 |
| `Napredni` | 3 |

### `TipCilja`
| Vrijednost | Int |
|-----------|-----|
| `Mrsavljenje` | 1 |
| `DobivanjeMase` | 2 |
| `PovecanjeSnage` | 3 |
| `Izdrzljivost` | 4 |
| `Fleksibilnost` | 5 |

---

## 3. Detalji tablica

### `Korisnici`

**Model:** `Models/Korisnik.cs`

| Stupac | C# tip | SQL tip | Nullable | Ograničenja |
|--------|--------|---------|----------|-------------|
| `Id` | `int` | `INT IDENTITY` | `NOT NULL` | PK |
| `Ime` | `string` | `NVARCHAR(50)` | `NOT NULL` | max 50 |
| `Prezime` | `string` | `NVARCHAR(50)` | `NOT NULL` | max 50 |
| `Email` | `string` | `NVARCHAR(MAX)` | `NOT NULL` | format e-pošte |
| `DatumRodjenja` | `DateTime` | `DATETIME2` | `NOT NULL` | — |
| `DatumRegistracije` | `DateTime` | `DATETIME2` | `NOT NULL` | default: `DateTime.Now` |
| `Visina` | `double` | `FLOAT` | `NOT NULL` | 100–250 (cm) |
| `Tezina` | `double` | `FLOAT` | `NOT NULL` | 30–300 (kg) |
| `PlanId` | `int?` | `INT` | `NULL` | FK → `Planovi.Id` |

**Navigacijska svojstva:**

| Property | Tip | Veza |
|----------|-----|------|
| `TrenutniPlan` | `Plan?` | N:1 prema `Planovi` |
| `Treninzi` | `ICollection<Trening>` | 1:N prema `Treninzi` |
| `Ciljevi` | `ICollection<Cilj>` | 1:N prema `Ciljevi` |
| `Mjerenja` | `ICollection<Mjerenje>` | 1:N prema `Mjerenja` |

---

### `Treninzi`

**Model:** `Models/Trening.cs`

| Stupac | C# tip | SQL tip | Nullable | Ograničenja |
|--------|--------|---------|----------|-------------|
| `Id` | `int` | `INT IDENTITY` | `NOT NULL` | PK |
| `DatumVrijeme` | `DateTime` | `DATETIME2` | `NOT NULL` | — |
| `TrajanjeMinuta` | `int` | `INT` | `NOT NULL` | 1–480 |
| `Vrsta` | `VrstaTreninga` | `INT` | `NOT NULL` | enum 1–5 |
| `Biljeske` | `string?` | `NVARCHAR(500)` | `NULL` | max 500 |
| `PotroseneKalorije` | `int?` | `INT` | `NULL` | 0–5000 |
| `Ocjena` | `int?` | `INT` | `NULL` | 1–10 |
| `KorisnikId` | `int` | `INT` | `NOT NULL` | FK → `Korisnici.Id` |

**Navigacijska svojstva:**

| Property | Tip | Veza |
|----------|-----|------|
| `Korisnik` | `Korisnik?` | N:1 prema `Korisnici` |
| `TreningVjezbe` | `ICollection<TreningVjezba>` | 1:N prema `TreningVjezbe` |

---

### `Vjezbe`

**Model:** `Models/Vjezba.cs`

| Stupac | C# tip | SQL tip | Nullable | Ograničenja |
|--------|--------|---------|----------|-------------|
| `Id` | `int` | `INT IDENTITY` | `NOT NULL` | PK |
| `Naziv` | `string` | `NVARCHAR(100)` | `NOT NULL` | max 100 |
| `Opis` | `string?` | `NVARCHAR(1000)` | `NULL` | max 1000 |
| `MisicnaSkupina` | `MisicnaSkupina` | `INT` | `NOT NULL` | enum 1–9 |
| `VideoUrl` | `string?` | `NVARCHAR(MAX)` | `NULL` | validan URL |
| `RazinaTezine` | `int` | `INT` | `NOT NULL` | 1–5 |
| `PotrebnaOprema` | `bool` | `BIT` | `NOT NULL` | — |

**Navigacijska svojstva:**

| Property | Tip | Veza |
|----------|-----|------|
| `TreningVjezbe` | `ICollection<TreningVjezba>` | 1:N prema `TreningVjezbe` |
| `Planovi` | `ICollection<Plan>` | N:N prema `Planovi` (kroz `PlanVjezba`) |

---

### `TreningVjezbe`

**Model:** `Models/TreningVjezba.cs`  
**Uloga:** Asocijativna tablica s payload-om — N:N između `Treninzi` i `Vjezbe` s detaljima izvedbe.

| Stupac | C# tip | SQL tip | Nullable | Ograničenja |
|--------|--------|---------|----------|-------------|
| `Id` | `int` | `INT IDENTITY` | `NOT NULL` | PK |
| `TreningId` | `int` | `INT` | `NOT NULL` | FK → `Treninzi.Id` |
| `VjezbaId` | `int` | `INT` | `NOT NULL` | FK → `Vjezbe.Id` |
| `BrojSerija` | `int` | `INT` | `NOT NULL` | 1–20 |
| `BrojPonavljanja` | `int` | `INT` | `NOT NULL` | 1–100 |
| `TezinaKg` | `double?` | `FLOAT` | `NULL` | 0–500 (kg) |
| `OdmorSekundi` | `int` | `INT` | `NOT NULL` | 0–600 (sek) |
| `Redoslijed` | `int` | `INT` | `NOT NULL` | 1–100 |

**Navigacijska svojstva:**

| Property | Tip | Veza |
|----------|-----|------|
| `Trening` | `Trening?` | N:1 prema `Treninzi` |
| `Vjezba` | `Vjezba?` | N:1 prema `Vjezbe` |

---

### `Planovi`

**Model:** `Models/Plan.cs`

| Stupac | C# tip | SQL tip | Nullable | Ograničenja |
|--------|--------|---------|----------|-------------|
| `Id` | `int` | `INT IDENTITY` | `NOT NULL` | PK |
| `Naziv` | `string` | `NVARCHAR(100)` | `NOT NULL` | max 100 |
| `Opis` | `string?` | `NVARCHAR(500)` | `NULL` | max 500 |
| `TrajanjeTjedana` | `int` | `INT` | `NOT NULL` | 1–52 |
| `Razina` | `RazinaIskustva` | `INT` | `NOT NULL` | enum 1–3 |
| `DatumKreiranja` | `DateTime` | `DATETIME2` | `NOT NULL` | default: `DateTime.Now` |

**Navigacijska svojstva:**

| Property | Tip | Veza |
|----------|-----|------|
| `Vjezbe` | `ICollection<Vjezba>` | N:N prema `Vjezbe` (kroz `PlanVjezba`) |
| `Korisnici` | `ICollection<Korisnik>` | 1:N prema `Korisnici` |

---

### `Ciljevi`

**Model:** `Models/Cilj.cs`

| Stupac | C# tip | SQL tip | Nullable | Ograničenja |
|--------|--------|---------|----------|-------------|
| `Id` | `int` | `INT IDENTITY` | `NOT NULL` | PK |
| `Tip` | `TipCilja` | `INT` | `NOT NULL` | enum 1–5 |
| `CiljanaVrijednost` | `double` | `FLOAT` | `NOT NULL` | 0.1–10000 |
| `Rok` | `DateTime` | `DATETIME2` | `NOT NULL` | — |
| `Postignut` | `bool` | `BIT` | `NOT NULL` | — |
| `KorisnikId` | `int` | `INT` | `NOT NULL` | FK → `Korisnici.Id` |

**Navigacijska svojstva:**

| Property | Tip | Veza |
|----------|-----|------|
| `Korisnik` | `Korisnik` | N:1 prema `Korisnici` |

---

### `Mjerenja`

**Model:** `Models/Mjerenje.cs`

| Stupac | C# tip | SQL tip | Nullable | Ograničenja |
|--------|--------|---------|----------|-------------|
| `Id` | `int` | `INT IDENTITY` | `NOT NULL` | PK |
| `DatumMjerenja` | `DateTime` | `DATETIME2` | `NOT NULL` | — |
| `Tezina` | `double` | `FLOAT` | `NOT NULL` | 30–300 (kg) |
| `PostotakMasti` | `double?` | `FLOAT` | `NULL` | 3–60 (%) |
| `OpsegStruka` | `double?` | `FLOAT` | `NULL` | 40–200 (cm) |
| `KorisnikId` | `int` | `INT` | `NOT NULL` | FK → `Korisnici.Id` |

**Navigacijska svojstva:**

| Property | Tip | Veza |
|----------|-----|------|
| `Korisnik` | `Korisnik?` | N:1 prema `Korisnici` |

---

### `PlanVjezba` *(auto-generirana join tablica)*

EF Core automatski kreira ovu tablicu iz N:N navigacijskih svojstava između `Plan` i `Vjezba`. Nema vlastitu C# klasu ni vlastiti `Id`.

| Stupac | SQL tip | Nullable | Ograničenja |
|--------|---------|----------|-------------|
| `PlanoviId` | `INT` | `NOT NULL` | PK (složeni), FK → `Planovi.Id` |
| `VjezbeId` | `INT` | `NOT NULL` | PK (složeni), FK → `Vjezbe.Id` |

---

## 4. Veze između entiteta

### 1:N veze

| Od | Prema | Strani ključ | Cascade |
|----|-------|-------------|---------|
| `Korisnici` | `Treninzi` | `Treninzi.KorisnikId` | CASCADE |
| `Korisnici` | `Ciljevi` | `Ciljevi.KorisnikId` | CASCADE |
| `Korisnici` | `Mjerenja` | `Mjerenja.KorisnikId` | CASCADE |
| `Planovi` | `Korisnici` | `Korisnici.PlanId` | NO ACTION (nullable) |
| `Treninzi` | `TreningVjezbe` | `TreningVjezbe.TreningId` | CASCADE |
| `Vjezbe` | `TreningVjezbe` | `TreningVjezbe.VjezbaId` | CASCADE |

### N:N veza

| Tablica 1 | Tablica 2 | Join tablica | Tip |
|-----------|-----------|-------------|-----|
| `Planovi` | `Vjezbe` | `PlanVjezba` *(auto)* | Čista N:N (bez payload-a) |
| `Treninzi` | `Vjezbe` | `TreningVjezbe` | N:N s payload-om (asocijativna klasa) |

---

## 5. ER dijagram

```
Planovi (1)
  │
  ├──[PlanVjezba]──── Vjezbe (N)
  │                      │
  │                      └──[TreningVjezbe]──── Treninzi (N)
  │                          (BrojSerija,              │
  │                           BrojPonavljanja,         │
  │                           TezinaKg...)             │
  │                                                    │
  └──────────── Korisnici (N) ───────────────────────(1)
                    │
                    ├──── Ciljevi (N)
                    └──── Mjerenja (N)
```

---

## 6. Kaskadno brisanje

| Brisanje entiteta | Automatski se brišu |
|-------------------|---------------------|
| `Korisnik` | svi `Treninzi`, `Ciljevi`, `Mjerenja` tog korisnika |
| `Trening` | svi `TreningVjezba` zapisi tog treninga |
| `Vjezba` | svi `TreningVjezba` zapisi te vježbe |
| `Plan` | **ne briše korisnike** — `PlanId` se postavi na `NULL` |

---

## 7. Seed data

Definirano u `TeretanaDbContext.OnModelCreating()`, primijenjeno migracijom `20260507100527_SeedData`.

### Vjezbe (8 zapisa)

| Id | Naziv | MisicnaSkupina | RazinaTezine | PotrebnaOprema |
|----|-------|---------------|-------------|----------------|
| 1 | Bench Press | Prsa (1) | 3 | Da |
| 2 | Čučnjevi | Noge (6) | 2 | Ne |
| 3 | Mrtvo dizanje | Leda (2) | 4 | Da |
| 4 | Biceps curl | Biceps (4) | 2 | Da |
| 5 | Plank | Trbuh (7) | 1 | Ne |
| 6 | Trčanje | Kardio (8) | 1 | Ne |
| 7 | Triceps dips | Triceps (5) | 2 | Da |
| 8 | Sklekovi | Prsa (1) | 1 | Ne |

### Planovi (3 zapisa)

| Id | Naziv | Razina | TrajanjeTjedana |
|----|-------|--------|----------------|
| 1 | Početni plan | Pocetnik (1) | 8 |
| 2 | Snaga i masa | Srednji (2) | 12 |
| 3 | Napredni program | Napredni (3) | 16 |

---

## 8. DbContext — DbSet svojstva

```csharp
// DAL/TeretanaDbContext.cs
public class TeretanaDbContext : DbContext
{
    public DbSet<Korisnik>      Korisnici    { get; set; }
    public DbSet<Trening>       Treninzi     { get; set; }
    public DbSet<Vjezba>        Vjezbe       { get; set; }
    public DbSet<TreningVjezba> TreningVjezbe { get; set; }
    public DbSet<Plan>          Planovi      { get; set; }
    public DbSet<Cilj>          Ciljevi      { get; set; }
    public DbSet<Mjerenje>      Mjerenja     { get; set; }
}
```

### Repository klase (registrirane kao `AddScoped`)

| Repository | Koristi Include |
|-----------|-----------------|
| `VjezbaRepository` | — (master data, bez veza) |
| `KorisnikRepository` | `Treninzi`, `Ciljevi`, `Mjerenja` |
| `TreningRepository` | `Korisnik`, `TreningVjezbe` → `Vjezba` |
| `PlanRepository` | `Vjezbe`, `Korisnici` |
| `CiljRepository` | `Korisnik` |
| `MjerenjeRepository` | `Korisnik` |
| `TreningVjezbaRepository` | `Trening`, `Vjezba` |

### Migracije

| Datoteka | Opis |
|----------|------|
| `20260426163820_InitialCreate` | Kreiranje svih 8 tablica i FK veza |
| `20260507100527_SeedData` | Umetanje 8 vježbi i 3 plana |
