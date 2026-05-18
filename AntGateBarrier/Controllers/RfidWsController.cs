using AntGateBarrier.SingletonServices;
using Microsoft.AspNetCore.Mvc;

namespace AntGateBarrier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RfidWsController : ControllerBase
    {
        private readonly IRfidWs _rfidWs;

        public RfidWsController(IRfidWs rfidWs)
        {
            _rfidWs = rfidWs;
        }

        [HttpGet]
        public async Task Get()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsync("Se requiere conexión WebSocket.");
                return;
            }

            // ── gate-location: header (Postman/API) o query param (browser) ──
            var gateRaw = HttpContext.Request.Headers["gate-location"].FirstOrDefault()
                       ?? HttpContext.Request.Query["gate-location"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(gateRaw) || !int.TryParse(gateRaw, out var gate))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsync(
                    "'gate-location' es requerido (header o query param) y debe ser un número entero.");
                return;
            }

            // ── api-key: header o query param (para uso futuro) ───────────────
            var apiKey = HttpContext.Request.Headers["api-key"].FirstOrDefault()
                      ?? HttpContext.Request.Query["api-key"].FirstOrDefault()
                      ?? string.Empty;

            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _rfidWs.HandleWebSocketConnection(webSocket, gate, apiKey);
        }
    }
}