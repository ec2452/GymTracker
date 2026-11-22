using System.Collections.Generic;
using System.Linq;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.DAL;
using Microsoft.EntityFrameworkCore;

namespace Sustavzapracenjenapretkauteretani.DAL.Repositories
{
    public class PlanRepository
    {
        private readonly TeretanaDbContext _context;

        public PlanRepository(TeretanaDbContext context)
        {
            _context = context;
        }

        public List<Plan> GetAll()
        {
            return _context.Planovi
                .Include(p => p.Vjezbe)
                .Include(p => p.Korisnici)
                .ToList();
        }

        public Plan? GetById(int id)
        {
            return _context.Planovi
                .Include(p => p.Vjezbe)
                .Include(p => p.Korisnici)
                .FirstOrDefault(p => p.Id == id);
        }

        public void Add(Plan plan)
        {
            _context.Planovi.Add(plan);
            _context.SaveChanges();
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var plan = _context.Planovi.FirstOrDefault(p => p.Id == id);
            if (plan != null)
            {
                var korisnici = _context.Korisnici.Where(k => k.PlanId == id).ToList();
                foreach (var k in korisnici)
                    k.PlanId = null;
                plan.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }
    }
}
