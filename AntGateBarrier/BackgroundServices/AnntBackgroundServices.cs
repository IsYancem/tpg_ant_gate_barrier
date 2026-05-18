
using AntGateBarrier.ScopedServices;

namespace AntGateBarrier.BackgroundServices
{
    public class AnntBackgroundServices : BackgroundService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IServiceProvider _serviceProvider;
        public AnntBackgroundServices(IServiceProvider serviceProvider) { _serviceProvider = serviceProvider; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _anntConfigScopedService = scope.ServiceProvider.GetRequiredService<IAnntScopedService>();
                    await _anntConfigScopedService.DoWorkAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception:{ex.Message}");
            }

        }
    }
}
