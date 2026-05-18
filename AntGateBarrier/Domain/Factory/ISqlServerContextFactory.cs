using AntGateBarrier.Domain.Context;

namespace AntGateBarrier.Domain.Factory
{
    public interface ISqlServerContextFactory
    {
        public SqlServerContext CreateDbContext();
    }
}
