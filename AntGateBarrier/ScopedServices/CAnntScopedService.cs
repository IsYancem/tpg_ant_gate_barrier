using AntGateBarrier.Domain.Controller;
using AntGateBarrier.Driver;
using AntGateBarrier.Entity;
using AntGateBarrier.SingletonServices;
using log4net;

namespace AntGateBarrier.ScopedServices
{
    public class CAnntScopedService : IAnntScopedService
    {
        private static readonly ILog _log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAppSettings _appSettings;
        private ISqlMemoryController _sqlMemoryController;
        private ISqlServerController _sqlServerController;
        private IRealtimeWs _realtimeWs;
        private IKioskoWs _kioskoWs;
        private IRfidWs _rfidWs;                        // ← NUEVO
        private IAnntSingletonService _anntSingletonServices;
        private IWeightSingletonService _weightSingletonService;
        private Settings? _settings { get; set; }
        private ConnectionStrings? _connectionStrings;

        public CAnntScopedService(
            IAppSettings appSettings,
            ISqlMemoryController sqlMemoryController,
            ISqlServerController sqlServerController,
            IRealtimeWs realtimeWs,
            IKioskoWs kioskoWs,
            IRfidWs rfidWs,                             // ← NUEVO parámetro
            IAnntSingletonService anntSingletonServices,
            IWeightSingletonService weightSingletonService)
        {
            _sqlMemoryController = sqlMemoryController;
            _sqlServerController = sqlServerController;
            _appSettings = appSettings;
            _realtimeWs = realtimeWs;
            _kioskoWs = kioskoWs;
            _rfidWs = rfidWs;
            _anntSingletonServices = anntSingletonServices;
            _weightSingletonService = weightSingletonService;
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
                    var _anntConfigs = await _sqlServerController.GetAnnts(_All, _settings.Group);
                    _log.Info($"{_anntConfigs.Count} records will be loaded Annt");

                    foreach (var _anntConfig in _anntConfigs)
                    {
                        if (await _anntSingletonServices.IsFoundAsync(_anntConfig.ip_address))
                        {
                            _log.Info($"It's already installed Anntena {_anntConfig.ip_address}");
                            if (_anntConfig.state == 0)
                            {
                                if (await _anntSingletonServices.DeleteAsync(_anntConfig.ip_address))
                                    _log.Info($"Delete Driver Anntena {_anntConfig.ip_address}");
                            }
                            else
                            {
                                if (await _anntSingletonServices.UpdateAsync(_anntConfig.ip_address, _anntConfig))
                                    _log.Info($"Update Driver Anntena {_anntConfig.ip_address}");
                            }
                        }
                        else
                        {
                            if (_anntConfig.state == 0)
                            {
                                _log.Info($"Could Not Add Anntena {_anntConfig.ip_address} - State {_anntConfig.state}");
                            }
                            else
                            {
                                // ← Se pasa _rfidWs al constructor
                                CRfid _irfid = new CRfid(
                                    _anntConfig,
                                    _sqlMemoryController,
                                    _sqlServerController,
                                    _realtimeWs,
                                    _kioskoWs,
                                    _weightSingletonService,
                                    _rfidWs);

                                var t = new Task(async () => { await _irfid.ServicesAsync(); });
                                t.Start();

                                var v = new Task(async () => { await _irfid.IsValidConnectionAsync(); });
                                v.Start();

                                if (await _anntSingletonServices.AddAsync(_anntConfig.ip_address, _irfid))
                                    _log.Info($"Add Driver Anntena {_anntConfig.ip_address}");
                                else
                                    _log.Info($"Could Not Add Anntena {_anntConfig.ip_address}");
                            }
                        }
                    }

                    _log.Info($"{_anntConfigs.Count} records loaded Annt");
                    _All = 1;
                    await Task.Delay(_connectionStrings.TimeQueryAnt, stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.Error($"Exception:{ex.Message}");
                }
            }
        }
    }
}