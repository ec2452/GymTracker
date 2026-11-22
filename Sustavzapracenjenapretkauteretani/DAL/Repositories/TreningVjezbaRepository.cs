using System.Collections.Generic;
using System.Linq;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.DAL;
using Microsoft.EntityFrameworkCore;

namespace Sustavzapracenjenapretkauteretani.DAL.Repositories
{
    public class TreningVjezbaRepository
    {
        private readonly TeretanaDbContext _context;

        public TreningVjezbaRepository(TeretanaDbContext context)
        {
            _context = context;
        }

        public List<TreningVjezba> GetAll()
        {
            return _context.TreningVjezbe
                .Include(tv => tv.Trening)
                .Include(tv => tv.Vjezba)
                .ToList();
        }

        public TreningVjezba? GetById(int id)
        {
            return _context.TreningVjezbe
                .Include(tv => tv.Trening)
                .Include(tv => tv.Vjezba)
                .FirstOrDefault(tv => tv.Id == id);
        }

        public void Add(TreningVjezba treningVjezba)
        {
            _context.TreningVjezbe.Add(treningVjezba);
            _context.SaveChanges();
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var treningVjezba = _context.TreningVjezbe.FirstOrDefault(tv => tv.Id == id);
            if (treningVjezba != null)
            {
                // Soft delete - postavi DeletedAt umjesto Remove
                treningVjezba.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }
    }
}
