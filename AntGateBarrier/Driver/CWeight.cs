using AntGateBarrier.Domain.Controller;
using AntGateBarrier.Domain.Model;
using AntGateBarrier.Entity;
using AntGateBarrier.SingletonServices;
using log4net;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AntGateBarrier.Driver
{
    public class CWeight : IWeight
    {
        protected static readonly ILog _log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private WeigthModel _weigthConfig;
        private readonly IWeightWs _weightWs;           // ← NUEVO

        private int _weight;
        private Socket _isClient { get; set; }
        private bool _isConnected { get; set; }
        private bool _isRunning { get; set; }

        public CWeight() { }

        public CWeight(WeigthModel weigthConfig, ISqlMemoryController sqlMemoryController,
            IWeightWs weightWs)                         // ← NUEVO parámetro
        {
            _weigthConfig = weigthConfig;
            _weightWs = weightWs;
            _weight = 0;
            _isRunning = true;
            _isConnected = false;
        }

        public async Task ConnectAsync()
        {
            _isConnected = false;
            try
            {
                IPAddress _ipAddress = IPAddress.Parse(_weigthConfig.ip_address);
                IPEndPoint _ipEndPoint = new IPEndPoint(_ipAddress, _weigthConfig.port);
                _isClient = new(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await _isClient.ConnectAsync(_ipEndPoint);
                _isConnected = true;
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex.Message} - {_weigthConfig.ip_address}");
                _isClient = null;
                _isConnected = false;
                _weight = 0;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            var _state = false;
            try
            {
                _isConnected = false;
                _weight = 0;
                _isClient.Close();
                _state = true;
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }

        public async Task<int> GetWeightAsync()
        {
            return _weight < 0 ? 0 : _weight;
        }

        public async Task<bool> IsConnectedAsync()
        {
            return _isConnected;
        }

        public async Task ServicesAsync()
        {
            while (_isRunning)
            {
                if (!(await IsConnectedAsync()))
                {
                    await ConnectAsync();
                    if (await IsConnectedAsync())
                        await ReadBufferAsync();
                }

                if (await IsConnectedAsync())
                {
                    try
                    {
                        var data = Encoding.ASCII.GetBytes("r wt0101\n");
                        await _isClient.SendAsync(data);

                        var recive = new byte[100];
                        var recvleng = await _isClient.ReceiveAsync(recive, SocketFlags.None);
                        var cmd = Encoding.ASCII.GetString(recive, 0, recvleng)
                            .Replace(">", "").Replace("\r", "").Replace("\n", "");

                        Console.WriteLine(cmd);

                        var arrayData = cmd.Split("~");
                        if (!(int.TryParse(arrayData[1], out _weight)))
                        {
                            _weight = 0;
                            _log.Warn($"ServicesAsync() - Parse fallido | Ip:{_weigthConfig.ip_address} | Raw:{cmd}");
                        }

                        _log.Info($"ip:{_weigthConfig.ip_address} / Weight:{_weight}");

                        // ── Publicar al WebSocket estructurado weightws ───────
                        // Solo se envía cuando hay conexión activa (Toledo conectado)
                        await PublishWeightWsAsync();
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Exception:{ex.Message}");
                        _weight = 0;
                        _isConnected = false;
                        // Sin conexión → no se publica nada al weightws
                    }
                }

                await Task.Delay(_weigthConfig.wait);
            }
        }

        /// <summary>
        /// Publica el peso actual al WebSocket estructurado (api/weightws).
        /// Se llama solo cuando _isConnected es true.
        /// </summary>
        private async Task PublishWeightWsAsync()
        {
            try
            {
                var wsMessage = new WeightWsMessage
                {
                    code = 0,
                    message = "Record Found",
                    gate = _weigthConfig.gate,
                    side = 0,                    // peso no tiene side, siempre 0
                    record = new WeightWsRecord
                    {
                        Weight = _weight < 0 ? 0 : _weight
                    }
                };
                var json = JsonConvert.SerializeObject(wsMessage);
                await _weightWs.SendMessageAsync(_weigthConfig.gate, json);
            }
            catch (Exception ex)
            {
                _log.Error($"PublishWeightWsAsync | Gate:{_weigthConfig.gate} | Exception:{ex.Message}");
            }
        }

        public async Task<bool> UpdateAsync(WeigthModel weigthConfig)
        {
            var _state = false;
            try
            {
                _weigthConfig.name = weigthConfig.name;
                _weigthConfig.ip_address = weigthConfig.ip_address;
                _weigthConfig.port = weigthConfig.port;
                _weigthConfig.wait = weigthConfig.wait;
                _state = true;
            }
            catch (Exception ex) { _log.Error($"Exception:{ex}"); }
            return _state;
        }

        private async Task ReadBufferAsync()
        {
            try
            {
                var recive = new byte[100];
                var maxleng = await _isClient.ReceiveAsync(recive, SocketFlags.None);
                var cmd = Encoding.ASCII.GetString(recive, 0, maxleng);

                var data = Encoding.ASCII.GetBytes("USER admin\n");
                await _isClient.SendAsync(data);

                maxleng = await _isClient.ReceiveAsync(recive, SocketFlags.None);
                cmd = Encoding.ASCII.GetString(recive, 0, maxleng);
                if (cmd.Contains("12")) { _log.Info("Access OK"); }
                else { _log.Warn("No Access"); }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex.Message}");
                _isConnected = false;
                _weight = 0;
            }
        }
    }
}