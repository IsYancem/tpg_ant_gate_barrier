using System.Net.WebSockets;

namespace AntGateBarrier.SingletonServices
{
    public interface IRfidWs
    {
        /// <summary>
        /// Registra el socket del cliente filtrando por gate-location.
        /// </summary>
        Task HandleWebSocketConnection(WebSocket socket, int gate, string apiKey);

        /// <summary>
        /// Envía un mensaje JSON solo a los clientes suscritos al gate indicado.
        /// </summary>
        Task SendMessageAsync(int gate, string message);
    }
}