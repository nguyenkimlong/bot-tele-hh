
using Microsoft.AspNetCore.Mvc;

namespace App_Driver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }
    }
}
