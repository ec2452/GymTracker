using System.Collections.Generic;
using System.Linq;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.DAL;
using Microsoft.EntityFrameworkCore;

namespace Sustavzapracenjenapretkauteretani.DAL.Repositories
{
    public class TreningRepository
    {
        private readonly TeretanaDbContext _context;

        public TreningRepository(TeretanaDbContext context)
        {
            _context = context;
        }

        public List<Trening> GetAll()
        {
            return _context.Treninzi
                .Include(t => t.Korisnik)
                .Include(t => t.TreningVjezbe)
                .ToList();
        }

        public Trening? GetById(int id)
        {
            return _context.Treninzi
                .Include(t => t.Korisnik)
                .Include(t => t.TreningVjezbe)
                    .ThenInclude(tv => tv.Vjezba)
                .FirstOrDefault(t => t.Id == id);
        }

        public void Add(Trening trening)
        {
            _context.Treninzi.Add(trening);
            _context.SaveChanges();
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var trening = _context.Treninzi.FirstOrDefault(t => t.Id == id);
            if (trening != null)
            {
                // Soft delete - postavi DeletedAt umjesto Remove
                trening.DeletedAt = DateTime.UtcNow;
                // Briši i sve povezane vježbe u treningu (soft delete)
                var treningVjezbe = _context.TreningVjezbe.Where(tv => tv.TreningId == id).ToList();
                foreach (var tv in treningVjezbe)
                {
                    tv.DeletedAt = DateTime.UtcNow;
                }
                _context.SaveChanges();
            }
        }
    }
}
