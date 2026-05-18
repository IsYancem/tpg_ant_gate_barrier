using AntGateBarrier.Domain.Context;

namespace AntGateBarrier.Domain.Factory
{
    public interface IMemoryContextFactory
    {
        public MemoryContext CreateDbContext();
    }
}
