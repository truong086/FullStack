using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.Applications.Base.Models;

namespace thuongmaidientus1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string criteria) => Ok();

        [HttpGet]
        public IActionResult GetPaging([FromQuery] BasePagingQuery basePaging)
        {
            return Ok();
        }
    }
}
