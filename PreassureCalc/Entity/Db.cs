using PreassureCalc.Models;
using System.Data.Entity;

namespace PreassureCalc.Entity
{
    public class Db : DbContext
    {
        public DbSet<Wells> Wells { get; set; }
        public Db() : base("DefaultConnection")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Wells>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
