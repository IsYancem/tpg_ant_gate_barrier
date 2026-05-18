using AntGateBarrier.Domain.Controller;
using AntGateBarrier.Driver;
using AntGateBarrier.Entity;
using AntGateBarrier.SingletonServices;
using log4net;

namespace AntGateBarrier.ScopedServices
{
    public class CWeightScopedService : IWeightScopedService
    {
        private static readonly ILog _log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAppSettings _appSettings;
        private ISqlMemoryController _sqlMemoryController;
        private ISqlServerController _sqlServerController;
        private IWeightSingletonService _weightSingletonService;
        private IWeightWs _weightWs;                    // ← NUEVO
        private Settings? _settings { get; set; }
        private ConnectionStrings? _connectionStrings;

        public CWeightScopedService(
            IAppSettings appSettings,
            ISqlMemoryController sqlMemoryController,
            ISqlServerController sqlServerController,
            IWeightSingletonService weightSingletonService,
            IWeightWs weightWs)                         // ← NUEVO parámetro
        {
            _sqlMemoryController = sqlMemoryController;
            _sqlServerController = sqlServerController;
            _appSettings = appSettings;
            _weightSingletonService = weightSingletonService;
            _weightWs = weightWs;
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
                    var _weightConfigs = await _sqlServerController.GetWeigths(_All, _settings.Group);
                    _log.Info($"{_weightConfigs.Count} records will be loaded Weight");

                    foreach (var _weightConfig in _weightConfigs)
                    {
                        if (await _weightSingletonService.IsFoundAsync(_weightConfig.gate))
                        {
                            _log.Info($"It's already installed Weight {_weightConfig.gate}");
                            if (_weightConfig.state == 0)
                            {
                                if (await _weightSingletonService.DeleteAsync(_weightConfig.gate))
                                    _log.Info($"Delete Driver Weight {_weightConfig.gate}");
                            }
                            else
                            {
                                if (await _weightSingletonService.UpdateAsync(_weightConfig.gate, _weightConfig))
                                    _log.Info($"Update Driver Weight {_weightConfig.gate}");
                            }
                        }
                        else
                        {
                            if (_weightConfig.state == 0)
                            {
                                _log.Info($"Could Not Add Weight {_weightConfig.gate} - State {_weightConfig.state}");
                            }
                            else
                            {
                                // ← Se pasa _weightWs al constructor
                                CWeight _cWeight = new CWeight(
                                    _weightConfig,
                                    _sqlMemoryController,
                                    _weightWs);

                                var t = new Task(async () => { await _cWeight.ServicesAsync(); });
                                t.Start();

                                if (await _weightSingletonService.AddAsync(_weightConfig.gate, _cWeight))
                                    _log.Info($"Add Driver Weight {_weightConfig.gate}");
                                else
                                    _log.Info($"Could Not Add Weight {_weightConfig.gate}");
                            }
                        }
                    }

                    _log.Info($"{_weightConfigs.Count} records loaded Weight");
                    _All = 1;
                    await Task.Delay(_connectionStrings.TimeQueryInd, stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.Error($"Exception:{ex.Message}");
                }
            }
        }
    }
}