
using AntGateBarrier.ScopedServices;

namespace AntGateBarrier.BackgroundServices
{
    public class WeightBackgroundServices : BackgroundService
    {
        private readonly log4net.ILog _log;
        private readonly IServiceProvider _serviceProvider;

        public WeightBackgroundServices(IServiceProvider serviceProvider)
        {
            _log = log4net.LogManager.GetLogger(GetType().Name);
            _serviceProvider = serviceProvider;
        }

        protected override async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _weigthScopedService = scope.ServiceProvider.GetRequiredService<IWeightScopedService>();
                    await _weigthScopedService.DoWorkAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex.Message}");
            }
        }
    }
}
