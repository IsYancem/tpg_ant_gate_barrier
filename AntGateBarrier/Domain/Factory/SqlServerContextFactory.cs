using AntGateBarrier.Domain.Context;
using AntGateBarrier.Entity;
using AntGateBarrier.SingletonServices;
using Microsoft.EntityFrameworkCore;

namespace AntGateBarrier.Domain.Factory
{
    public class SqlServerContextFactory : ISqlServerContextFactory
    {
        protected readonly IAppSettings _appSettings;
        protected readonly ConnectionStrings? _connectionStrings;

        public SqlServerContextFactory(IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _connectionStrings = _appSettings.GetConnectionStrings();
        }
        public SqlServerContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlServerContext>();
            optionsBuilder.UseSqlServer(_appSettings.Decode(_connectionStrings.SqlServer), providerOptions => providerOptions.EnableRetryOnFailure());
            return new SqlServerContext(optionsBuilder.Options);
        }
    }
}
