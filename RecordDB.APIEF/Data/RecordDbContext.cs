using Microsoft.EntityFrameworkCore;
using RecordDB.APIEF.Models;
using System.Reflection;

namespace RecordDB.APIEF.Data
{
    public class RecordDbContext(DbContextOptions<RecordDbContext> options) : DbContext(options)
    {
        public DbSet<Artist> Artists => Set<Artist>();
        public DbSet<Record> Records => Set<Record>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
