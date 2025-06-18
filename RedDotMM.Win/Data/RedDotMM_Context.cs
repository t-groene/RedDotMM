using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using RedDotMM;
using RedDotMM.Model;
using Microsoft.EntityFrameworkCore;

namespace RedDotMM.Win.Data
{
    public class RedDotMM_Context : DbContext
    {

        public DbSet<Ergebnis> Ergebnisse { get; set; }
        public DbSet<Schuetze> Schuetzen { get; set; }

        public DbSet<Serie> Serien { get; set; }
        public DbSet<Wettbewerb> Wettbewerbe { get; set; }
        public DbSet<Schuss> Schuesse { get; set; }

        public RedDotMM_Context() : base()
        {

        }

        public RedDotMM_Context(DbContextOptions<RedDotMM_Context> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {



            optionsBuilder.UseSqlite("Data Source=RedDotMM_Datenbank.sqlite");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }

}

