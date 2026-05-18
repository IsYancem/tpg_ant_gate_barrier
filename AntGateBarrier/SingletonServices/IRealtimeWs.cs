using System.Net.WebSockets;

namespace AntGateBarrier.SingletonServices
{
    public interface IRealtimeWs
    {
        public Task HandleWebSocketConnection(WebSocket socket);
        public Task SendMessage(string message);
    }
}
