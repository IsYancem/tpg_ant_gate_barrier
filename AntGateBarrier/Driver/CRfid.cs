using AntGateBarrier.Domain.Controller;
using AntGateBarrier.Domain.Model;
using AntGateBarrier.Entity;
using AntGateBarrier.SingletonServices;
using log4net;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;

namespace AntGateBarrier.Driver
{
    public class CRfid : IRfid
    {
        protected static readonly ILog _log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AnntModel _anntConfig;
        private readonly ISqlMemoryController _sqlMemoryController;
        private readonly ISqlServerController _sqlServerController;
        private readonly IRealtimeWs _realtimeWs;
        private readonly IKioskoWs _kioskoWs;
        private readonly IRfidWs _rfidWs;
        private readonly IWeightSingletonService _weightSingletonService;
        private IAppSettings _appSettings;

        private Socket _isClient { get; set; }
        private bool _isRunning { get; set; }

        public CRfid() { }

        public CRfid(
            AnntModel anntConfig,
            ISqlMemoryController sqlMemoryController,
            ISqlServerController sqlServerController,
            IRealtimeWs realtimeWs,
            IKioskoWs kioskoWs,
            IWeightSingletonService weightSingletonService,
            IRfidWs rfidWs)
        {
            _anntConfig = anntConfig;
            _sqlMemoryController = sqlMemoryController;
            _sqlServerController = sqlServerController;
            _realtimeWs = realtimeWs;
            _kioskoWs = kioskoWs;
            _weightSingletonService = weightSingletonService;
            _rfidWs = rfidWs;
            _isRunning = true;
            _appSettings = new CAppSettings();
        }

        public async Task ConnectAsync()
        {
            try
            {
                IPAddress _ipAddress = IPAddress.Parse(_anntConfig.ip_address);
                IPEndPoint _ipEndPoint = new IPEndPoint(_ipAddress, _anntConfig.port);
                _isClient = new(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await _isClient.ConnectAsync(_ipEndPoint);
                _isClient.ReceiveTimeout = 3000;
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex.Message} - {_anntConfig.ip_address}");
                _isClient = null;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            var _state = false;
            try
            {
                if (_isClient != null)
                {
                    _isClient.Shutdown(SocketShutdown.Both);
                    _isClient = null;
                }
                _state = true;
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }

        public async Task ServicesAsync()
        {
            while (_isRunning)
            {
                var _anntRfid = new AnntRfidModel();
                _anntRfid.IpAddress = _anntConfig.ip_address;
                _anntRfid.Now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                _anntRfid.gate = _anntConfig.gate;
                _anntRfid.Name = _anntConfig.name;
                _anntRfid.Side = await _appSettings.SideToString(_anntConfig.side);
                _anntRfid.Site = await _appSettings.SiteToString(_anntConfig.site);

                VehicleModel _currentVehicle = null;

                try
                {
                    _anntRfid.Code = 1;

                    if (!await IsConnectedAsync())
                    {
                        await ConnectAsync();
                        if (!await IsConnectedAsync())
                            _anntRfid.Message = "Offline ServicesAsync";
                    }

                    if (await IsConnectedAsync())
                    {
                        var bytessize = _isClient.Available;
                        if (bytessize == 0)
                        {
                            _anntRfid.Code = 2;
                            _anntRfid.Message = "Searching for TAG";
                            _log.Info($"Searching for TAG Ip:{_anntConfig.ip_address} - Gate:{_anntConfig.name}");
                        }
                        else
                        {
                            var buffer = new byte[1024];
                            var received = await _isClient.ReceiveAsync(
                                new ArraySegment<byte>(buffer), SocketFlags.None);
                            var data = BitConverter.ToString(buffer).Replace("-", string.Empty);
                            var datas = data.Split(_anntConfig.search);
                            var frames = datas
                                .Where(r => (r.Length - _anntConfig.first) >= _anntConfig.max)
                                .ToList();
                            var fixframes = frames
                                .Select(r => r.Substring(_anntConfig.first, _anntConfig.max))
                                .Distinct()
                                .ToList();

                            foreach (var _frame in fixframes)
                            {
                                _log.Info($"Searching Ip:{_anntConfig.ip_address} - Gate:{_anntConfig.name} - Frame:{_frame}");
                                _anntRfid.Code = 1;
                                _anntRfid.Message = "Search Rfid";
                                _anntRfid.Hexa = _frame;
                                _anntRfid.Now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

                                var _weight = await _weightSingletonService.GetWeightAsync(_anntConfig.gate);
                                if (_weight < 0 || _weight > 0)
                                {
                                    _anntRfid.Code = 1;
                                    _anntRfid.Message = $"The Weight {_weight} is greater than 0";
                                    _log.Warn($"The Weight {_weight} is greater than 0 - Gate:{_anntConfig.name}");
                                }
                                else
                                {
                                    VehicleModel _vehicle = await _sqlMemoryController.GetVehicle(
                                        _frame, _anntConfig.last);

                                    if (_vehicle == null)
                                    {
                                        _anntRfid.Code = 1;
                                        _anntRfid.Message = "Rfid Not Found";
                                        _log.Warn($"Rfid Not Found {_frame} - Ip:{_anntConfig.ip_address} - Gate:{_anntConfig.name}");
                                    }
                                    else
                                    {
                                        _currentVehicle = _vehicle;
                                        _anntRfid.Plate = _vehicle.REGNUMBER;
                                        _anntRfid.Number = _vehicle.RFID;
                                        _log.Info($"Plate:{_vehicle.REGNUMBER} - Msg:{_vehicle.MESSAGE} - Frame:{_frame}");

                                        // ── Publicar al WebSocket estructurado rfidws ─────
                                        await PublishRfidWsAsync(_vehicle, _anntConfig.gate, _anntConfig.side);

                                        if (_vehicle.STATE == 0)
                                        {
                                            _anntRfid.Code = 1;
                                            _anntRfid.Message = _vehicle.MESSAGE;
                                            _log.Warn($"Plate:{_vehicle.REGNUMBER} - Gate:{_anntConfig.name} - Msg:{_vehicle.MESSAGE}");
                                        }
                                        else
                                        {
                                            _log.Info($"Query PostAuth Gate:{_anntConfig.gate} Plate:{_vehicle.REGNUMBER}");
                                            var _responseMethod = await _appSettings.PostAuth(
                                                _vehicle.REGNUMBER, _anntConfig.gate, _anntConfig.side,
                                                _anntConfig.site, _frame, _anntConfig.ip_address,
                                                _anntConfig.ipaddress_barrier,
                                                _anntConfig.url_auth, _anntConfig.key_auth);

                                            _log.Info($"Execute PostAuth Gate:{_anntConfig.gate} Plate:{_vehicle.REGNUMBER} Code:{_responseMethod.Code}");

                                            if (_responseMethod.Code == 1)
                                            {
                                                _anntRfid.Code = 1;
                                                _anntRfid.Message = _responseMethod.Msg;
                                                _log.Warn($"Plate:{_vehicle.REGNUMBER} - Gate:{_anntConfig.name} - Msg:{_responseMethod.Msg}");
                                            }
                                            else
                                            {
                                                _anntRfid.IsRegistered = 1;
                                                _anntRfid.Registered = $"Plate {_vehicle.REGNUMBER} registered at {_anntRfid.Now}";
                                                _log.Info($"Query PostOpenBarrier Gate:{_anntConfig.gate} Plate:{_vehicle.REGNUMBER}");

                                                if (_anntConfig.isplc == 0)
                                                {
                                                    var _responseMethodOpen = await _appSettings.PostUnLockGateC3(
                                                        _vehicle.REGNUMBER, _anntConfig.gate, _anntConfig.side,
                                                        _anntConfig.ipaddress_barrier,
                                                        _anntConfig.url_barrier, _anntConfig.key_barrier);

                                                    if (_responseMethodOpen.Code == 1)
                                                    {
                                                        _anntRfid.Code = 1;
                                                        _anntRfid.Message = _responseMethodOpen.Msg;
                                                        _log.Warn($"Plate {_vehicle.REGNUMBER} registered/{_responseMethodOpen.Msg}");
                                                    }
                                                    else
                                                    {
                                                        _anntRfid.Code = 0;
                                                        _anntRfid.Message = $"Open Barrier {await _appSettings.SideToString(_anntConfig.side)}";
                                                        _log.Info($"Open Barrier {_responseMethod.Msg}");
                                                    }
                                                }
                                                else if (_anntConfig.isplc == 1)
                                                {
                                                    var _responseMethodOpen = await _appSettings.PostUnLockGatePLC(
                                                        _vehicle.REGNUMBER, _anntConfig.gate, _anntConfig.side,
                                                        _anntConfig.url_barrier, _anntConfig.key_barrier);

                                                    if (_responseMethodOpen.Code == 1)
                                                    {
                                                        _anntRfid.Code = 1;
                                                        _anntRfid.Message = _responseMethodOpen.Msg;
                                                        _log.Warn($"Plate {_vehicle.REGNUMBER} registered/{_responseMethodOpen.Msg}");
                                                    }
                                                    else
                                                    {
                                                        _anntRfid.Code = 0;
                                                        _anntRfid.Message = $"Open Barrier {await _appSettings.SideToString(_anntConfig.side)}";
                                                        _log.Info($"Open Barrier {_responseMethod.Msg}");
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"ServicesAsync() - Ip:{_anntConfig.ip_address} - Exception:{ex.Message}");
                    _anntRfid.Code = 1;
                    _anntRfid.Message = ex.Message;
                    await DisconnectAsync();
                }

                try
                {
                    await _sqlMemoryController.AddAnntRfid(_anntRfid);

                    // ── Realtime WS (broadcast general, ya existía) ──
                    var jsonrealtime = JsonConvert.SerializeObject(_anntRfid);
                    await _realtimeWs.SendMessage(jsonrealtime);

                    // ── RfidWs: SIEMPRE publicar, todos los estados ──────────────
                    var rfidWsMsg = new RfidWsMessage
                    {
                        code = _anntRfid.Code,
                        message = _anntRfid.Message,
                        gate = _anntRfid.gate,
                        side = _anntConfig.side,
                        record = _currentVehicle == null ? null : new RfidWsRecord
                        {
                            RegNumber = _currentVehicle.REGNUMBER,
                            Rfid = _currentVehicle.RFID,
                            Brand = _currentVehicle.BRAND,
                            Model = _currentVehicle.MODEL,
                            Color = _currentVehicle.COLOR,
                            Company = _currentVehicle.COMPANY,
                            Hexastring = _currentVehicle.HEXASTRING,
                            Message = _currentVehicle.MESSAGE,
                            IdVehicle = _currentVehicle.IDVEHICLE,
                            IdCompany = _currentVehicle.IDCOMPANY,
                            State = _currentVehicle.STATE
                        }
                    };
                    await _rfidWs.SendMessageAsync(_anntRfid.gate, JsonConvert.SerializeObject(rfidWsMsg));
                    // ─────────────────────────────────────────────────────────────

                    await Task.Delay(_anntConfig.wait);

                    CKiosko _cKiosko = new CKiosko
                    {
                        code = _anntRfid.Code,
                        msg = _anntRfid.Message,
                        gate = _anntRfid.gate,
                        plate = _anntRfid.Plate,
                        side = _anntConfig.side,
                        site = _anntConfig.site
                    };
                    var jsonkiosko = JsonConvert.SerializeObject(_cKiosko);
                    await _kioskoWs.SendMessage(jsonkiosko);
                    await Task.Delay(_anntConfig.wait);
                }
                catch (Exception ex)
                {
                    _log.Error($"ServicesAsync() - Ip:{_anntConfig.ip_address} - Exception:{ex.Message}");
                    _anntRfid.Code = 1;
                    _anntRfid.Message = ex.Message;
                    await DisconnectAsync();
                }
            }
        }

        /// <summary>
        /// Recibe VehicleModel (tipado fuerte) — ya no usa dynamic.
        /// VehicleModel debe tener IDVEHICLE e IDCOMPANY mapeados desde el SP.
        /// </summary>
        private async Task PublishRfidWsAsync(VehicleModel vehicle, int gate, int side)
        {
            try
            {
                var wsMessage = new RfidWsMessage
                {
                    code = 0,
                    message = "Record Found",
                    gate = gate,
                    side = side,
                    record = new RfidWsRecord
                    {
                        RegNumber = vehicle.REGNUMBER,
                        Rfid = vehicle.RFID,
                        Brand = vehicle.BRAND,
                        Model = vehicle.MODEL,
                        Color = vehicle.COLOR,
                        Company = vehicle.COMPANY,
                        Hexastring = vehicle.HEXASTRING,
                        Message = vehicle.MESSAGE,
                        IdVehicle = vehicle.IDVEHICLE,
                        IdCompany = vehicle.IDCOMPANY,
                        State = vehicle.STATE
                    }
                };
                var json = JsonConvert.SerializeObject(wsMessage);
                await _rfidWs.SendMessageAsync(gate, json);

                _log.Info($"[RfidWs] Publicado | Gate:{gate} | Plate:{vehicle.REGNUMBER}");
            }
            catch (Exception ex)
            {
                _log.Error($"PublishRfidWsAsync | Gate:{gate} | Exception:{ex.Message}");
            }
        }

        public async Task<bool> UpdateAsync(AnntModel anntConfig)
        {
            var _state = false;
            try
            {
                _anntConfig.port = anntConfig.port;
                _anntConfig.side = anntConfig.side;
                _anntConfig.ipaddress_barrier = anntConfig.ipaddress_barrier;
                _anntConfig.search = anntConfig.search;
                _anntConfig.first = anntConfig.first;
                _anntConfig.last = anntConfig.last;
                _anntConfig.max = anntConfig.max;
                _anntConfig.wait = anntConfig.wait;
                _anntConfig.sensor = anntConfig.sensor;
                _anntConfig.isplc = anntConfig.isplc;
                _anntConfig.url_auth = anntConfig.url_auth;
                _anntConfig.key_auth = anntConfig.key_auth;
                _anntConfig.url_barrier = anntConfig.url_barrier;
                _anntConfig.key_barrier = anntConfig.key_barrier;
                _anntConfig.site = anntConfig.site;
                _state = true;
            }
            catch (Exception ex) { _log.Error($"Exception:{ex}"); }
            return _state;
        }

        public async Task<bool> IsConnectedAsync()
        {
            if (_isClient == null) return false;
            return _isClient.Connected;
        }

        public async Task IsValidConnectionAsync()
        {
            while (_isRunning)
            {
                try
                {
                    if (_isClient != null)
                    {
                        _log.Info($"SynchronizingService client disconnected Ip:{_anntConfig.ip_address}");
                        await DisconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"IsValidConnectionAsync() - Ip:{_anntConfig.ip_address} Exception:{ex.Message}");
                    await DisconnectAsync();
                }
                await Task.Delay(_anntConfig.sensor);
            }
        }
    }
}