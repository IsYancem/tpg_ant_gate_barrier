using AntGateBarrier.Entity;
using log4net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace AntGateBarrier.SingletonServices
{
    public class CKioskoWs : IKioskoWs
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<WebSocket> _sockets { get; set; }
        public CKioskoWs() { _sockets = new(); }

        public async Task HandleWebSocketConnection(WebSocket socket)
        {
            _log.Info($"ADD USER WEBSOCKER");
            _sockets.Add(socket);
            var buffer = new byte[1024 * 2];
            try
            {
                while (socket.State == WebSocketState.Open)
                {

                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), default);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, default);
                        break;
                    }

                }

            }
            catch { }
            _log.Info($"DELETE USER WEBSOCKER");
            _sockets.Remove(socket);
        }

        public async Task SendMessage(string message)
        {
            if (_sockets.Count > 0)
            {
                foreach (var s in _sockets)
                {
                    await s.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, default);
                }
            }
        }
    }
}

