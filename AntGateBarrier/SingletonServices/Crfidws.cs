using log4net;
using System.Net.WebSockets;
using System.Text;

namespace AntGateBarrier.SingletonServices
{
    public class CRfidWs : IRfidWs
    {
        private static readonly ILog _log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // gate → lista de sockets suscritos a ese gate
        private readonly Dictionary<int, List<WebSocket>> _socketsByGate = new();
        private readonly object _lock = new();

        public async Task HandleWebSocketConnection(WebSocket socket, int gate, string apiKey)
        {
            lock (_lock)
            {
                if (!_socketsByGate.ContainsKey(gate))
                    _socketsByGate[gate] = new List<WebSocket>();
                _socketsByGate[gate].Add(socket);
            }
            _log.Info($"[RfidWs] Cliente conectado | Gate:{gate}");

            var buffer = new byte[1024 * 2];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), default);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(result.CloseStatus.Value,
                            result.CloseStatusDescription, default);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"[RfidWs] Socket cerrado inesperadamente | Gate:{gate} | {ex.Message}");
            }

            lock (_lock)
            {
                if (_socketsByGate.TryGetValue(gate, out var list))
                {
                    list.Remove(socket);
                    if (list.Count == 0)
                        _socketsByGate.Remove(gate);
                }
            }
            _log.Info($"[RfidWs] Cliente desconectado | Gate:{gate}");
        }

        public async Task SendMessageAsync(int gate, string message)
        {
            List<WebSocket> targets;
            lock (_lock)
            {
                if (!_socketsByGate.TryGetValue(gate, out var list) || list.Count == 0)
                    return;
                targets = new List<WebSocket>(list);
            }

            var bytes = Encoding.UTF8.GetBytes(message);
            var dead = new List<WebSocket>();

            foreach (var socket in targets)
            {
                try
                {
                    if (socket.State == WebSocketState.Open)
                        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, default);
                    else
                        dead.Add(socket);
                }
                catch
                {
                    dead.Add(socket);
                }
            }

            if (dead.Count > 0)
            {
                lock (_lock)
                {
                    if (_socketsByGate.TryGetValue(gate, out var list))
                        dead.ForEach(d => list.Remove(d));
                }
            }
        }
    }
}