using System.Collections.Generic;
using System.Linq;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.DAL;

namespace Sustavzapracenjenapretkauteretani.DAL.Repositories
{
    public class VjezbaRepository
    {
        private readonly TeretanaDbContext _context;

        public VjezbaRepository(TeretanaDbContext context)
        {
            _context = context;
        }

        public List<Vjezba> GetAll()
        {
            return _context.Vjezbe.ToList();
        }

        public Vjezba? GetById(int id)
        {
            return _context.Vjezbe.FirstOrDefault(v => v.Id == id);
        }

        public void Add(Vjezba vjezba)
        {
            _context.Vjezbe.Add(vjezba);
            _context.SaveChanges();
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var vjezba = _context.Vjezbe.Find(id);
            if (vjezba != null)
            {
                var now = DateTime.UtcNow;
                var treningVjezbe = _context.TreningVjezbe.Where(tv => tv.VjezbaId == id).ToList();
                foreach (var tv in treningVjezbe)
                    tv.DeletedAt = now;
                vjezba.DeletedAt = now;
                _context.SaveChanges();
            }
        }
    }
}
