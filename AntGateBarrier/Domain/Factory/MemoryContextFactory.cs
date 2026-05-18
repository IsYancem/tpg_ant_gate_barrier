using AntGateBarrier.Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace AntGateBarrier.Domain.Factory
{
    public class MemoryContextFactory : IMemoryContextFactory
    {
        public MemoryContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MemoryContext>();
            optionsBuilder.UseInMemoryDatabase("ZkMemory");

            return new MemoryContext(optionsBuilder.Options);
        }
    }
}
