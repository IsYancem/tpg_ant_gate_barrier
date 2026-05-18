using AntGateBarrier.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace AntGateBarrier.Domain.Context
{
    public class SqlServerContext : DbContext
    {
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }
        public DbSet<VehicleModel> Vehicles { get; set; }
        public DbSet<AnntModel> Annts { get; set; }
        public DbSet<WeigthModel> Weigths { get; set; }
    }
}
