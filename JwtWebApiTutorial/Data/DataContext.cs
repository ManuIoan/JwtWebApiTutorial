using JwtWebApiTutorial.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtWebApiTutorial.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<Addresses> Addresses { get; set; }
        public DbSet<UAT> UAT { get; set; }
    }
}
