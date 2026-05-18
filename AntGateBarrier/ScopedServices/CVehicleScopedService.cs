
using AntGateBarrier.Domain.Controller;
using AntGateBarrier.Entity;
using AntGateBarrier.SingletonServices;
using log4net;
using System.Reflection;

namespace AntGateBarrier.ScopedServices
{
    public class CVehicleScopedService : IVehicleScopedService
    {
        private readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IAppSettings _appSettings;
        private ISqlMemoryController _sqlMemoryController;
        private ISqlServerController _sqlServerController;
        private Settings? _settings { get; set; }
        private ConnectionStrings? _connectionStrings;

        public CVehicleScopedService(ISqlMemoryController sqlMemoryController, ISqlServerController sqlServerController, IAppSettings appSettings) 
        {
            _sqlMemoryController = sqlMemoryController;
            _sqlServerController = sqlServerController;
            _appSettings = appSettings;
            _settings = _appSettings.GetSettings();
            _connectionStrings = _appSettings.GetConnectionStrings();
        }   
        
        public async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            int _All = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var _vehicles = await _sqlServerController.GetVehicles(_All);
                    _log.Info($"{_vehicles.Count} records will be loaded Vehicle");
                    foreach (var _vehicle in _vehicles)
                    {
                        await _sqlMemoryController.AddVehicle(_vehicle);
                    }
                    _log.Info($"{_vehicles.Count} records loaded Vehicle");
                    _All = 1;
                    await Task.Delay(_connectionStrings.TimeQueryVehicle, stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.Error($"Exception:{ex}");
                }
            }
        }
    }
}
