using System.Collections.Generic;
using System.Linq;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.DAL;
using Microsoft.EntityFrameworkCore;

namespace Sustavzapracenjenapretkauteretani.DAL.Repositories
{
    public class KorisnikRepository
    {
        private readonly TeretanaDbContext _context;

        public KorisnikRepository(TeretanaDbContext context)
        {
            _context = context;
        }

        public List<Korisnik> GetAll()
        {
            return _context.Korisnici.ToList();
        }

        public Korisnik? GetById(int id)
        {
            return _context.Korisnici
                .Include(k => k.Treninzi)
                .Include(k => k.Ciljevi)
                .Include(k => k.Mjerenja)
                .AsSplitQuery()
                .FirstOrDefault(k => k.Id == id);
        }

        public void Add(Korisnik korisnik)
        {
            _context.Korisnici.Add(korisnik);
            _context.SaveChanges();
        }

        public void Update()
        {
            _context.SaveChanges();
        }

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

        /// <summary>
        /// Trajno briše korisnika i sve povezane zapise
        /// NAPOMENA: Koristi se samo u iznimnim slučajevima
        /// </summary>
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
    }
}
