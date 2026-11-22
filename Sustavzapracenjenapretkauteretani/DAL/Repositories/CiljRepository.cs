using System.Collections.Generic;
using System.Linq;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.DAL;
using Microsoft.EntityFrameworkCore;

namespace Sustavzapracenjenapretkauteretani.DAL.Repositories
{
    public class CiljRepository
    {
        private readonly TeretanaDbContext _context;

        public CiljRepository(TeretanaDbContext context)
        {
            _context = context;
        }

        public List<Cilj> GetAll()
        {
            return _context.Ciljevi
                .Include(c => c.Korisnik)
                .ToList();
        }

        public Cilj? GetById(int id)
        {
            return _context.Ciljevi
                .Include(c => c.Korisnik)
                .FirstOrDefault(c => c.Id == id);
        }

        public void Add(Cilj cilj)
        {
            _context.Ciljevi.Add(cilj);
            _context.SaveChanges();
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var cilj = _context.Ciljevi.FirstOrDefault(c => c.Id == id);
            if (cilj != null)
            {
                // Soft delete - postavi DeletedAt umjesto Remove
                cilj.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }
    }
}
