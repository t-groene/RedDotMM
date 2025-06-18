using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Data
{
    public class RedDotMMContext : DbContext
    {
        public DbSet<Model.Ergebnis> Ergebnisse { get; set; }
        public DbSet<Model.Schuetze> Schuetzen { get; set; }
        public DbSet<Model.Wettbewerb> Wettbewerbe { get; set; }
        public DbSet<Model.Schuss> Schuesse { get; set; }

        public RedDotMMContext() :base()
        {

        }

        public RedDotMMContext(DbContextOptions<RedDotMMContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {



            optionsBuilder.UseSqlite("Data Source=RedDotMM.sqlite");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}
