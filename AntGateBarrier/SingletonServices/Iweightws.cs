using System.Net.WebSockets;

namespace AntGateBarrier.SingletonServices
{
    public interface IWeightWs
    {
        Task HandleWebSocketConnection(WebSocket socket, int gate, string apiKey);
        Task SendMessageAsync(int gate, string message);
    }
}