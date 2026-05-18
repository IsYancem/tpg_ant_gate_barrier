using AntGateBarrier.SingletonServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AntGateBarrier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WsController : ControllerBase
    {
        private readonly IRealtimeWs _realtimeWs;
        public WsController(IRealtimeWs realtimeWs)
        {
            _realtimeWs = realtimeWs;
        }

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _realtimeWs.HandleWebSocketConnection(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }

        }
    }
}
