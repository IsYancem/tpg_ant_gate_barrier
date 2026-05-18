using AntGateBarrier.SingletonServices;
using Microsoft.AspNetCore.Mvc;

namespace AntGateBarrier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KioskoController : ControllerBase
    {
        private readonly IKioskoWs _kioskoWs;
        private readonly IAppSettings _appSettings;

        public KioskoController(IKioskoWs kioskoWs, IAppSettings appSettings)
        {
            _kioskoWs = kioskoWs;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task Get()
        {
            string KEY = _appSettings.GetSettings().ApiKey;
            string ApiKey = HttpContext.Request.Headers["ApiKey"];

            if (ApiKey == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status203NonAuthoritative;
            }
            else if (!ApiKey.Equals(KEY))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status203NonAuthoritative;
            }
            else
            {
                if (HttpContext.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    await _kioskoWs.HandleWebSocketConnection(webSocket);
                }
                else
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }

        }
    }
}
