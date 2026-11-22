using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Sustavzapracenjenapretkauteretani.Web.Models;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VjezbaRepository _vjezbaRepository;
        private readonly PlanRepository _planRepository;
        private readonly KorisnikRepository _korisnikRepository;

        public HomeController(
            ILogger<HomeController> logger,
            VjezbaRepository vjezbaRepository,
            PlanRepository planRepository,
            KorisnikRepository korisnikRepository)
        {
            _logger = logger;
            _vjezbaRepository = vjezbaRepository;
            _planRepository = planRepository;
            _korisnikRepository = korisnikRepository;
        }

        [HttpGet("", Name = "HomeRoot")]
        [HttpGet("pocetna", Name = "HomePocetna")]
        public IActionResult Index()
        {
            return View(CreateModel());
        }

        private HomeDashboardViewModel CreateModel()
        {
            var now = DateTime.Now;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            var thirtyDaysAgo = now.AddDays(-30);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var vjezbe = _vjezbaRepository.GetAll().ToList();
            var planovi = _planRepository.GetAll().ToList();
            var korisnici = _korisnikRepository.GetAll().ToList();

            var treninzi = korisnici
                .SelectMany(k => k.Treninzi)
                .Where(t => t.DeletedAt == null)
                .ToList();

            var ciljevi = korisnici
                .SelectMany(k => k.Ciljevi)
                .Where(c => c.DeletedAt == null)
                .ToList();

            var mjerenja = korisnici
                .SelectMany(k => k.Mjerenja)
                .Where(m => m.DeletedAt == null)
                .OrderByDescending(m => m.DatumMjerenja)
                .ToList();

            // Težina stats
            var lastMjerenje = mjerenja.FirstOrDefault();
            var mjerenje30DanaAgo = mjerenja.LastOrDefault(m => m.DatumMjerenja <= thirtyDaysAgo);
            double? tezinaDelta = (lastMjerenje != null && mjerenje30DanaAgo != null)
                ? lastMjerenje.Tezina - mjerenje30DanaAgo.Tezina
                : null;

            // Ciljana težina — uzimamo zadnji cilj tipa Mrsavljenje ili DobivanjeMase
            var tezinskoKilj = ciljevi
                .Where(c => c.Tip == Teretana.Models.TipCilja.Mrsavljenje
                         || c.Tip == Teretana.Models.TipCilja.DobivanjeMase)
                .OrderByDescending(c => c.Rok)
                .FirstOrDefault();

            // Sparkline iz zadnjih 12 mjerenja
            var sparklineWeights = mjerenja
                .Take(12)
                .Select(m => m.Tezina)
                .Reverse()
                .ToList();

            // Treninzi stats
            int tjedan = treninzi.Count(t => t.DatumVrijeme >= startOfWeek);
            double? prosjecnaOcjena = treninzi.Any(t => t.Ocjena.HasValue)
                ? treninzi.Where(t => t.Ocjena.HasValue).Average(t => (double)t.Ocjena!.Value)
                : null;
            int prosjecnoTrajanje = treninzi.Any()
                ? (int)treninzi.Average(t => t.TrajanjeMinuta)
                : 0;

            // Korisnici ovaj mjesec
            int noviOvajMjesec = korisnici.Count(k => k.DeletedAt == null
                && k.DatumRegistracije >= startOfMonth);

            return new HomeDashboardViewModel
            {
                VjezbeCount = vjezbe.Count(v => v.DeletedAt == null),
                PlanoviCount = planovi.Count(p => p.DeletedAt == null),
                KorisniciCount = korisnici.Count(k => k.DeletedAt == null),
                TreninziCount = treninzi.Count,
                CiljeviCount = ciljevi.Count,
                MjerenjaCount = mjerenja.Count,

                ZadnjaTezina = lastMjerenje?.Tezina,
                TezinaDelta30Dana = tezinaDelta,
                ZadnjeMjerenjeDate = lastMjerenje?.DatumMjerenja,
                CiljanaTezina = tezinskoKilj?.CiljanaVrijednost,
                SparklinePoints = ComputeSparkline(sparklineWeights),

                TreninziOvajTjedan = tjedan,
                ProsjecnaOcjena = prosjecnaOcjena,
                ProsjecnoTrajanje = prosjecnoTrajanje,

                CiljeviPostignutiCount = ciljevi.Count(c => c.Postignut),

                KorisniciOvajMjesecCount = noviOvajMjesec,
            };
        }

        private static string ComputeSparkline(List<double> weights, int width = 220, int height = 56)
        {
            if (weights.Count < 2) return string.Empty;
            var min = weights.Min();
            var max = weights.Max();
            var range = max - min;
            if (range < 0.5) range = 0.5;

            var points = weights.Select((w, i) =>
            {
                var x = (double)i / (weights.Count - 1) * width;
                var y = height - ((w - min) / range * (height - 10) + 5);
                return $"{x:F1},{y:F1}";
            });

            return string.Join(" ", points);
        }

        [HttpGet("privatnost", Name = "HomePrivacy")]
        public IActionResult Privacy()
        {
            var viewModel = new PrivacyViewModel
            {
                Title = "Privacy Policy",
                Description = "Use this page to detail your site's privacy policy."
            };

            return View(viewModel);
        }

        [HttpGet("greska", Name = "HomeError")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
