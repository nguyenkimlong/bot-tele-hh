
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace App_Driver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public HomeController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }

        [HttpGet("restart")]
        public async Task<IActionResult> RestartBot()
        {
            var myBackgroundService = _serviceProvider.GetServices<IHostedService>()
               .OfType<App_Driver.Worker.Worker>()
               .First();

            try
            {
                myBackgroundService.RestartTask();
                return Ok($"MyBackgroundService was restarted");
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
