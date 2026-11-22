using System.Collections.Generic;
using System.Linq;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.DAL;
using Microsoft.EntityFrameworkCore;

namespace Sustavzapracenjenapretkauteretani.DAL.Repositories
{
    public class MjerenjeRepository
    {
        private readonly TeretanaDbContext _context;

        public MjerenjeRepository(TeretanaDbContext context)
        {
            _context = context;
        }

        public List<Mjerenje> GetAll()
        {
            return _context.Mjerenja
                .Include(m => m.Korisnik)
                .ToList();
        }

        public Mjerenje? GetById(int id)
        {
            return _context.Mjerenja
                .Include(m => m.Korisnik)
                .FirstOrDefault(m => m.Id == id);
        }

        public void Add(Mjerenje mjerenje)
        {
            _context.Mjerenja.Add(mjerenje);
            _context.SaveChanges();
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var mjerenje = _context.Mjerenja.FirstOrDefault(m => m.Id == id);
            if (mjerenje != null)
            {
                // Soft delete - postavi DeletedAt umjesto Remove
                mjerenje.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }
    }
}
