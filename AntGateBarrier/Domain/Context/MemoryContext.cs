using AntGateBarrier.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace AntGateBarrier.Domain.Context
{
    public class MemoryContext : DbContext
    {
        public MemoryContext(DbContextOptions<MemoryContext> options) : base(options) { }
        public DbSet<VehicleModel> Vehicles { get; set; }
        public DbSet<AnntRfidModel> AnntRfids { get; set;}
        public DbSet<WeigthModel> Weigths { get; set; }
    }
}
