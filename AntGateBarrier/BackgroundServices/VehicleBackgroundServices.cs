
using AntGateBarrier.ScopedServices;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace AntGateBarrier.BackgroundServices
{
    public class VehicleBackgroundServices : BackgroundService
    {
        private readonly log4net.ILog _log;
        private readonly IServiceProvider _serviceProvider;
        
        public VehicleBackgroundServices(IServiceProvider serviceProvider) 
        {
            _log = log4net.LogManager.GetLogger(GetType().Name);
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _vehicleScopedService = scope.ServiceProvider.GetRequiredService<IVehicleScopedService>();
                    await _vehicleScopedService.DoWorkAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex}");
            }
        }
    }
}
