using AntGateBarrier.Domain.Controller;
using AntGateBarrier.Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AntGateBarrier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RfidController : ControllerBase
    {
        private readonly ISqlMemoryController _sqlMemoryController;

        public RfidController(ISqlMemoryController sqlMemoryController)
        {
            _sqlMemoryController = sqlMemoryController;
        }

        [HttpGet]
        public async Task<ActionResult<List<AnntRfidModel>>> Index()
        {
            var _anntRfids = await _sqlMemoryController.GetAnntRfids();
            return _anntRfids;
        }
    }
}
