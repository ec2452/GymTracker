using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.DAL
{
    public class TeretanaDbContext : IdentityDbContext<AppUser>
    {
        public TeretanaDbContext(DbContextOptions<TeretanaDbContext> options)
            : base(options) { }

        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Trening> Treninzi { get; set; }
        public DbSet<Vjezba> Vjezbe { get; set; }
        public DbSet<TreningVjezba> TreningVjezbe { get; set; }
        public DbSet<Plan> Planovi { get; set; }
        public DbSet<Cilj> Ciljevi { get; set; }
        public DbSet<Mjerenje> Mjerenja { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============ KORISNIK - FLUENT API OGRANIČENJA ============
            modelBuilder.Entity<Korisnik>()
                .Property(k => k.Ime)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Korisnik>()
                .Property(k => k.Prezime)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Korisnik>()
                .Property(k => k.Email)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Korisnik>()
                .HasIndex(k => k.Email)
                .IsUnique();

            modelBuilder.Entity<Korisnik>()
                .Property(k => k.DatumRodjenja)
                .IsRequired();

            modelBuilder.Entity<Korisnik>()
                .Property(k => k.DatumRegistracije)
                .IsRequired();

            modelBuilder.Entity<Korisnik>()
                .Property(k => k.Visina)
                .IsRequired();

            modelBuilder.Entity<Korisnik>()
                .Property(k => k.Tezina)
                .IsRequired()
                .HasPrecision(5, 2);

            // ============ TRENING - FLUENT API OGRANIČENJA ============
            modelBuilder.Entity<Trening>()
                .Property(t => t.KorisnikId)
                .IsRequired();

            modelBuilder.Entity<Trening>()
                .Property(t => t.DatumVrijeme)
                .IsRequired();

            modelBuilder.Entity<Trening>()
                .Property(t => t.TrajanjeMinuta)
                .IsRequired();

            modelBuilder.Entity<Trening>()
                .Property(t => t.Vrsta)
                .IsRequired();

            modelBuilder.Entity<Trening>()
                .Property(t => t.Biljeske)
                .HasMaxLength(1000);

            modelBuilder.Entity<Trening>()
                .Property(t => t.PotroseneKalorije)
                .HasPrecision(6, 1);

            // ============ VJEZBA - FLUENT API OGRANIČENJA ============
            modelBuilder.Entity<Vjezba>()
                .Property(v => v.Naziv)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Vjezba>()
                .HasIndex(v => v.Naziv)
                .IsUnique();

            modelBuilder.Entity<Vjezba>()
                .Property(v => v.Opis)
                .HasMaxLength(1000);

            modelBuilder.Entity<Vjezba>()
                .Property(v => v.MisicnaSkupina)
                .IsRequired();

            modelBuilder.Entity<Vjezba>()
                .Property(v => v.VideoUrl)
                .HasMaxLength(2000);

            modelBuilder.Entity<Vjezba>()
                .Property(v => v.RazinaTezine)
                .IsRequired();

            modelBuilder.Entity<Vjezba>()
                .Property(v => v.PotrebnaOprema)
                .IsRequired()
                .HasDefaultValue(false);

            // ============ CILJ - FLUENT API OGRANIČENJA ============
            modelBuilder.Entity<Cilj>()
                .Property(c => c.KorisnikId)
                .IsRequired();

            modelBuilder.Entity<Cilj>()
                .Property(c => c.Tip)
                .IsRequired();

            modelBuilder.Entity<Cilj>()
                .Property(c => c.CiljanaVrijednost)
                .IsRequired()
                .HasPrecision(10, 2);

            modelBuilder.Entity<Cilj>()
                .Property(c => c.Rok)
                .IsRequired();

            modelBuilder.Entity<Cilj>()
                .Property(c => c.Postignut)
                .IsRequired()
                .HasDefaultValue(false);

            // ============ MJERENJE - FLUENT API OGRANIČENJA ============
            modelBuilder.Entity<Mjerenje>()
                .Property(m => m.KorisnikId)
                .IsRequired();

            modelBuilder.Entity<Mjerenje>()
                .Property(m => m.DatumMjerenja)
                .IsRequired();

            modelBuilder.Entity<Mjerenje>()
                .Property(m => m.Tezina)
                .IsRequired()
                .HasPrecision(5, 2);

            modelBuilder.Entity<Mjerenje>()
                .Property(m => m.PostotakMasti)
                .HasPrecision(4, 1);

            modelBuilder.Entity<Mjerenje>()
                .Property(m => m.OpsegStruka)
                .HasPrecision(6, 1);

            // ============ PLAN - FLUENT API OGRANIČENJA ============
            modelBuilder.Entity<Plan>()
                .Property(p => p.Naziv)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Plan>()
                .Property(p => p.Opis)
                .HasMaxLength(2000);

            modelBuilder.Entity<Plan>()
                .Property(p => p.TrajanjeTjedana)
                .IsRequired();

            modelBuilder.Entity<Plan>()
                .Property(p => p.Razina)
                .IsRequired();

            modelBuilder.Entity<Plan>()
                .Property(p => p.DatumKreiranja)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // ============ TRENING VJEZBA - FLUENT API OGRANIČENJA ============
            modelBuilder.Entity<TreningVjezba>()
                .Property(tv => tv.TreningId)
                .IsRequired();

            modelBuilder.Entity<TreningVjezba>()
                .Property(tv => tv.VjezbaId)
                .IsRequired();

            modelBuilder.Entity<TreningVjezba>()
                .Property(tv => tv.BrojSerija)
                .IsRequired();

            modelBuilder.Entity<TreningVjezba>()
                .Property(tv => tv.BrojPonavljanja)
                .IsRequired();

            modelBuilder.Entity<TreningVjezba>()
                .Property(tv => tv.TezinaKg)
                .HasPrecision(6, 2);

            modelBuilder.Entity<TreningVjezba>()
                .Property(tv => tv.OdmorSekundi)
                .IsRequired();

            modelBuilder.Entity<TreningVjezba>()
                .Property(tv => tv.Redoslijed)
                .IsRequired();

            // ============ ATTACHMENT - FLUENT API ============
            modelBuilder.Entity<Attachment>()
                .Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Attachment>()
                .Property(a => a.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<Attachment>()
                .Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Attachment>()
                .Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // ============ GLOBAL QUERY FILTERS - SOFT DELETE ============
            // Automatski filtrira obrisane zapise na svim upitima
            modelBuilder.Entity<Korisnik>()
                .HasQueryFilter(k => k.DeletedAt == null);

            modelBuilder.Entity<Cilj>()
                .HasQueryFilter(c => c.DeletedAt == null);

            modelBuilder.Entity<Trening>()
                .HasQueryFilter(t => t.DeletedAt == null);

            modelBuilder.Entity<Vjezba>()
                .HasQueryFilter(v => v.DeletedAt == null);

            modelBuilder.Entity<Mjerenje>()
                .HasQueryFilter(m => m.DeletedAt == null);

            modelBuilder.Entity<Plan>()
                .HasQueryFilter(p => p.DeletedAt == null);

            modelBuilder.Entity<TreningVjezba>()
                .HasQueryFilter(tv => tv.DeletedAt == null);

            // Attachment nema vlastiti DeletedAt, ali ovisi o Vjezba koja ima soft delete.
            // Ovaj filter osigurava da se ne vraćaju attachmenti čija vježba je soft-deletana,
            // čime se rješava EF Core upozorenje o nekonzistentnom query filteru.
            modelBuilder.Entity<Attachment>()
                .HasQueryFilter(a => a.Vjezba!.DeletedAt == null);

            // ============ SEED DATA ============
            modelBuilder.Entity<Vjezba>().HasData(
                new Vjezba { Id = 1, Naziv = "Bench Press",    Opis = "Vježba za prsa na ravnoj klupi s šipkom ili bučicama.", MisicnaSkupina = MisicnaSkupina.Prsa,    RazinaTezine = 3, PotrebnaOprema = true  },
                new Vjezba { Id = 2, Naziv = "Čučnjevi",       Opis = "Temeljna vježba za noge i gluteus.",                   MisicnaSkupina = MisicnaSkupina.Noge,    RazinaTezine = 2, PotrebnaOprema = false },
                new Vjezba { Id = 3, Naziv = "Mrtvo dizanje",  Opis = "Složena vježba za leđa, noge i ruke.",                 MisicnaSkupina = MisicnaSkupina.Leda,    RazinaTezine = 4, PotrebnaOprema = true  },
                new Vjezba { Id = 4, Naziv = "Biceps curl",    Opis = "Izolirana vježba za biceps s bučicama ili šipkom.",    MisicnaSkupina = MisicnaSkupina.Biceps,  RazinaTezine = 2, PotrebnaOprema = true  },
                new Vjezba { Id = 5, Naziv = "Plank",          Opis = "Statička vježba za stabilizaciju trbuha i trupa.",     MisicnaSkupina = MisicnaSkupina.Trbuh,   RazinaTezine = 1, PotrebnaOprema = false },
                new Vjezba { Id = 6, Naziv = "Trčanje",        Opis = "Aerobna aktivnost za kardiovaskularni sustav.",        MisicnaSkupina = MisicnaSkupina.Kardio,  RazinaTezine = 1, PotrebnaOprema = false },
                new Vjezba { Id = 7, Naziv = "Triceps dips",   Opis = "Vježba za triceps na paralelnim šipkama.",             MisicnaSkupina = MisicnaSkupina.Triceps, RazinaTezine = 2, PotrebnaOprema = true  },
                new Vjezba { Id = 8, Naziv = "Sklekovi",       Opis = "Vježba s vlastitom težinom za prsa i triceps.",        MisicnaSkupina = MisicnaSkupina.Prsa,    RazinaTezine = 1, PotrebnaOprema = false }
            );

            modelBuilder.Entity<Plan>().HasData(
                new Plan { Id = 1, Naziv = "Početni plan",     Opis = "Idealan za početnike koji tek kreću s vježbanjem.", TrajanjeTjedana = 8,  Razina = RazinaIskustva.Pocetnik, DatumKreiranja = new DateTime(2025, 1, 1) },
                new Plan { Id = 2, Naziv = "Snaga i masa",     Opis = "Plan usmjeren na povećanje mišićne mase i jakosti.", TrajanjeTjedana = 12, Razina = RazinaIskustva.Srednji,  DatumKreiranja = new DateTime(2025, 1, 1) },
                new Plan { Id = 3, Naziv = "Napredni program", Opis = "Intenzivan plan za iskusne vježbače.",               TrajanjeTjedana = 16, Razina = RazinaIskustva.Napredni, DatumKreiranja = new DateTime(2025, 1, 1) }
            );

            modelBuilder.Entity<Korisnik>().HasData(
                new Korisnik { Id = 1, Ime = "Marko",  Prezime = "Horvat",  Email = "marko.horvat@email.com",  DatumRodjenja = new DateTime(1995, 3, 12), DatumRegistracije = new DateTime(2025, 1, 1), Visina = 182, Tezina = 88, PlanId = 2 },
                new Korisnik { Id = 2, Ime = "Ana",    Prezime = "Kovač",   Email = "ana.kovac@email.com",     DatumRodjenja = new DateTime(1998, 7, 25), DatumRegistracije = new DateTime(2025, 1, 1), Visina = 165, Tezina = 62, PlanId = 1 },
                new Korisnik { Id = 3, Ime = "Tomislav",Prezime = "Blažić", Email = "tomislav.blazic@email.com",DatumRodjenja = new DateTime(1990, 11, 8), DatumRegistracije = new DateTime(2025, 1, 1), Visina = 178, Tezina = 95, PlanId = 3 }
            );

            modelBuilder.Entity<Cilj>().HasData(
                new Cilj { Id = 1, Tip = TipCilja.Mrsavljenje,    CiljanaVrijednost = 80,  Rok = new DateTime(2025, 6, 1),  Postignut = false, KorisnikId = 1 },
                new Cilj { Id = 2, Tip = TipCilja.PovecanjeSnage, CiljanaVrijednost = 120, Rok = new DateTime(2025, 9, 1),  Postignut = false, KorisnikId = 1 },
                new Cilj { Id = 3, Tip = TipCilja.Mrsavljenje,    CiljanaVrijednost = 57,  Rok = new DateTime(2025, 5, 1),  Postignut = true,  KorisnikId = 2 },
                new Cilj { Id = 4, Tip = TipCilja.Fleksibilnost,  CiljanaVrijednost = 90,  Rok = new DateTime(2025, 12, 1), Postignut = false, KorisnikId = 2 },
                new Cilj { Id = 5, Tip = TipCilja.DobivanjeMase,  CiljanaVrijednost = 100, Rok = new DateTime(2025, 8, 1),  Postignut = false, KorisnikId = 3 },
                new Cilj { Id = 6, Tip = TipCilja.Izdrzljivost,   CiljanaVrijednost = 60,  Rok = new DateTime(2025, 10, 1), Postignut = false, KorisnikId = 3 }
            );

            modelBuilder.Entity<Mjerenje>().HasData(
                new Mjerenje { Id = 1, DatumMjerenja = new DateTime(2025, 1, 15), Tezina = 88,  PostotakMasti = 22, OpsegStruka = 95,  KorisnikId = 1 },
                new Mjerenje { Id = 2, DatumMjerenja = new DateTime(2025, 2, 15), Tezina = 86,  PostotakMasti = 21, OpsegStruka = 93,  KorisnikId = 1 },
                new Mjerenje { Id = 3, DatumMjerenja = new DateTime(2025, 3, 15), Tezina = 84,  PostotakMasti = 20, OpsegStruka = 90,  KorisnikId = 1 },
                new Mjerenje { Id = 4, DatumMjerenja = new DateTime(2025, 4, 15), Tezina = 82,  PostotakMasti = 19, OpsegStruka = 88,  KorisnikId = 1 },
                new Mjerenje { Id = 5, DatumMjerenja = new DateTime(2025, 1, 15), Tezina = 62,  PostotakMasti = 24, OpsegStruka = 72,  KorisnikId = 2 },
                new Mjerenje { Id = 6, DatumMjerenja = new DateTime(2025, 2, 15), Tezina = 61,  PostotakMasti = 23, OpsegStruka = 70,  KorisnikId = 2 },
                new Mjerenje { Id = 7, DatumMjerenja = new DateTime(2025, 3, 15), Tezina = 59,  PostotakMasti = 22, OpsegStruka = 68,  KorisnikId = 2 },
                new Mjerenje { Id = 8, DatumMjerenja = new DateTime(2025, 4, 15), Tezina = 57,  PostotakMasti = 21, OpsegStruka = 65,  KorisnikId = 2 },
                new Mjerenje { Id = 9, DatumMjerenja = new DateTime(2025, 1, 15), Tezina = 95,  PostotakMasti = 28, OpsegStruka = 110, KorisnikId = 3 },
                new Mjerenje { Id = 10, DatumMjerenja = new DateTime(2025, 2, 15), Tezina = 97,  PostotakMasti = 27, OpsegStruka = 112, KorisnikId = 3 },
                new Mjerenje { Id = 11, DatumMjerenja = new DateTime(2025, 3, 15), Tezina = 99,  PostotakMasti = 26, OpsegStruka = 115, KorisnikId = 3 },
                new Mjerenje { Id = 12, DatumMjerenja = new DateTime(2025, 4, 15), Tezina = 101, PostotakMasti = 25, OpsegStruka = 118, KorisnikId = 3 }
            );

            // ============ TRENINZI SEED ============
            modelBuilder.Entity<Trening>().HasData(
                // Marko Horvat (KorisnikId=1) — plan Snaga i masa
                new Trening { Id = 1, DatumVrijeme = new DateTime(2025, 1, 20, 18, 0, 0), TrajanjeMinuta = 75, Vrsta = VrstaTreninga.Snaga,       KorisnikId = 1, PotroseneKalorije = 450, Ocjena = 8, Biljeske = "Odličan trening snage, osjetio se napredak." },
                new Trening { Id = 2, DatumVrijeme = new DateTime(2025, 2, 10, 17, 30, 0), TrajanjeMinuta = 80, Vrsta = VrstaTreninga.Snaga,      KorisnikId = 1, PotroseneKalorije = 480, Ocjena = 9, Biljeske = "Novi osobni rekord na bench pressu — 90 kg!" },
                new Trening { Id = 3, DatumVrijeme = new DateTime(2025, 3,  5, 19, 0, 0), TrajanjeMinuta = 45, Vrsta = VrstaTreninga.HIIT,        KorisnikId = 1, PotroseneKalorije = 550, Ocjena = 7, Biljeske = "Naporno ali učinkovito za mršavljenje." },
                // Ana Kovač (KorisnikId=2) — plan Početni plan
                new Trening { Id = 4, DatumVrijeme = new DateTime(2025, 1, 25, 10, 0, 0), TrajanjeMinuta = 60, Vrsta = VrstaTreninga.Funkcionalni, KorisnikId = 2, PotroseneKalorije = 320, Ocjena = 7, Biljeske = "Dobra funkcionalna sesija za cijelo tijelo." },
                new Trening { Id = 5, DatumVrijeme = new DateTime(2025, 2, 20, 9, 30, 0),  TrajanjeMinuta = 45, Vrsta = VrstaTreninga.Kardio,     KorisnikId = 2, PotroseneKalorije = 380, Ocjena = 8, Biljeske = "Lagano trčanje u parku, odlično za odmor." },
                new Trening { Id = 6, DatumVrijeme = new DateTime(2025, 3, 15, 11, 0, 0), TrajanjeMinuta = 50, Vrsta = VrstaTreninga.Mobility,    KorisnikId = 2, PotroseneKalorije = 180, Ocjena = 9, Biljeske = "Odlično istezanje, osjećam se fleksibilnije." },
                // Tomislav Blažić (KorisnikId=3) — plan Napredni program
                new Trening { Id = 7, DatumVrijeme = new DateTime(2025, 1, 22, 17, 0, 0), TrajanjeMinuta = 90, Vrsta = VrstaTreninga.Snaga,       KorisnikId = 3, PotroseneKalorije = 620, Ocjena = 9, Biljeske = "Teška sesija za gornji dio tijela, maksimalne težine." },
                new Trening { Id = 8, DatumVrijeme = new DateTime(2025, 2, 18, 18, 0, 0), TrajanjeMinuta = 50, Vrsta = VrstaTreninga.HIIT,        KorisnikId = 3, PotroseneKalorije = 680, Ocjena = 8, Biljeske = "Kondicionalni trening s visokim intenzitetom." },
                new Trening { Id = 9, DatumVrijeme = new DateTime(2025, 3, 20, 17, 30, 0), TrajanjeMinuta = 85, Vrsta = VrstaTreninga.Snaga,      KorisnikId = 3, PotroseneKalorije = 590, Ocjena = 8, Biljeske = "Fokus na noge i mrtvo dizanje — solidan napredak." }
            );

            // ============ TRENING VJEZBE SEED ============
            modelBuilder.Entity<TreningVjezba>().HasData(
                // Trening 1 — Marko, Snaga: Bench Press, Čučnjevi, Triceps dips, Plank
                new TreningVjezba { Id = 1,  TreningId = 1, VjezbaId = 1, BrojSerija = 4, BrojPonavljanja = 8,  TezinaKg = 80,   OdmorSekundi = 120, Redoslijed = 1 },
                new TreningVjezba { Id = 2,  TreningId = 1, VjezbaId = 2, BrojSerija = 4, BrojPonavljanja = 10, TezinaKg = 100,  OdmorSekundi = 120, Redoslijed = 2 },
                new TreningVjezba { Id = 3,  TreningId = 1, VjezbaId = 7, BrojSerija = 3, BrojPonavljanja = 12, TezinaKg = null, OdmorSekundi = 90,  Redoslijed = 3 },
                new TreningVjezba { Id = 4,  TreningId = 1, VjezbaId = 5, BrojSerija = 3, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 60,  Redoslijed = 4 },
                // Trening 2 — Marko, Snaga: Bench Press, Mrtvo dizanje, Biceps curl
                new TreningVjezba { Id = 5,  TreningId = 2, VjezbaId = 1, BrojSerija = 4, BrojPonavljanja = 6,  TezinaKg = 90,   OdmorSekundi = 180, Redoslijed = 1 },
                new TreningVjezba { Id = 6,  TreningId = 2, VjezbaId = 3, BrojSerija = 4, BrojPonavljanja = 5,  TezinaKg = 120,  OdmorSekundi = 180, Redoslijed = 2 },
                new TreningVjezba { Id = 7,  TreningId = 2, VjezbaId = 4, BrojSerija = 3, BrojPonavljanja = 12, TezinaKg = 20,   OdmorSekundi = 90,  Redoslijed = 3 },
                // Trening 3 — Marko, HIIT: Trčanje, Sklekovi, Plank
                new TreningVjezba { Id = 8,  TreningId = 3, VjezbaId = 6, BrojSerija = 5, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 60,  Redoslijed = 1 },
                new TreningVjezba { Id = 9,  TreningId = 3, VjezbaId = 8, BrojSerija = 4, BrojPonavljanja = 15, TezinaKg = null, OdmorSekundi = 45,  Redoslijed = 2 },
                new TreningVjezba { Id = 10, TreningId = 3, VjezbaId = 5, BrojSerija = 3, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 30,  Redoslijed = 3 },
                // Trening 4 — Ana, Funkcionalni: Sklekovi, Čučnjevi, Plank
                new TreningVjezba { Id = 11, TreningId = 4, VjezbaId = 8, BrojSerija = 3, BrojPonavljanja = 10, TezinaKg = null, OdmorSekundi = 60,  Redoslijed = 1 },
                new TreningVjezba { Id = 12, TreningId = 4, VjezbaId = 2, BrojSerija = 3, BrojPonavljanja = 15, TezinaKg = null, OdmorSekundi = 60,  Redoslijed = 2 },
                new TreningVjezba { Id = 13, TreningId = 4, VjezbaId = 5, BrojSerija = 3, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 45,  Redoslijed = 3 },
                // Trening 5 — Ana, Kardio: Trčanje
                new TreningVjezba { Id = 14, TreningId = 5, VjezbaId = 6, BrojSerija = 1, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 0,   Redoslijed = 1 },
                // Trening 6 — Ana, Mobility: Plank
                new TreningVjezba { Id = 15, TreningId = 6, VjezbaId = 5, BrojSerija = 4, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 30,  Redoslijed = 1 },
                // Trening 7 — Tomislav, Snaga: Bench Press, Mrtvo dizanje, Biceps curl, Triceps dips
                new TreningVjezba { Id = 16, TreningId = 7, VjezbaId = 1, BrojSerija = 5, BrojPonavljanja = 5,  TezinaKg = 110,  OdmorSekundi = 180, Redoslijed = 1 },
                new TreningVjezba { Id = 17, TreningId = 7, VjezbaId = 3, BrojSerija = 5, BrojPonavljanja = 3,  TezinaKg = 160,  OdmorSekundi = 240, Redoslijed = 2 },
                new TreningVjezba { Id = 18, TreningId = 7, VjezbaId = 4, BrojSerija = 4, BrojPonavljanja = 10, TezinaKg = 25,   OdmorSekundi = 90,  Redoslijed = 3 },
                new TreningVjezba { Id = 19, TreningId = 7, VjezbaId = 7, BrojSerija = 4, BrojPonavljanja = 12, TezinaKg = null, OdmorSekundi = 90,  Redoslijed = 4 },
                // Trening 8 — Tomislav, HIIT: Trčanje, Sklekovi, Plank
                new TreningVjezba { Id = 20, TreningId = 8, VjezbaId = 6, BrojSerija = 6, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 60,  Redoslijed = 1 },
                new TreningVjezba { Id = 21, TreningId = 8, VjezbaId = 8, BrojSerija = 5, BrojPonavljanja = 20, TezinaKg = null, OdmorSekundi = 45,  Redoslijed = 2 },
                new TreningVjezba { Id = 22, TreningId = 8, VjezbaId = 5, BrojSerija = 4, BrojPonavljanja = 1,  TezinaKg = null, OdmorSekundi = 30,  Redoslijed = 3 },
                // Trening 9 — Tomislav, Snaga: Čučnjevi, Mrtvo dizanje, Triceps dips
                new TreningVjezba { Id = 23, TreningId = 9, VjezbaId = 2, BrojSerija = 5, BrojPonavljanja = 5,  TezinaKg = 140,  OdmorSekundi = 240, Redoslijed = 1 },
                new TreningVjezba { Id = 24, TreningId = 9, VjezbaId = 3, BrojSerija = 4, BrojPonavljanja = 4,  TezinaKg = 150,  OdmorSekundi = 240, Redoslijed = 2 },
                new TreningVjezba { Id = 25, TreningId = 9, VjezbaId = 7, BrojSerija = 3, BrojPonavljanja = 15, TezinaKg = null, OdmorSekundi = 90,  Redoslijed = 3 }
            );
        }
    }
}
