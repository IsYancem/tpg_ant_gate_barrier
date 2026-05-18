using AntGateBarrier.Entity;
using System.Net.WebSockets;

namespace AntGateBarrier.SingletonServices
{
    public interface IKioskoWs
    {
        public Task HandleWebSocketConnection(WebSocket socket);
        public Task SendMessage(string message);
    }
}
