using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderService.Application.Common;
using OrderService.Application.DTO;
using OrderService.Application.Handler.GetOrderById;
using OrderService.Application.Interface.Cache;
using OrderService.Application.Interface.RateLimit;
using System.Text.Json.Serialization;

namespace OrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IRateLimiter _limiter;

        public TestController(ILogger<TestController> logger, IRateLimiter limiter)
        {
            _logger = logger;
            _limiter = limiter;
        }


        [HttpPost("rate-limit")]
        public async Task<IActionResult> RateLimit_Test()
        {
            Console.WriteLine($"{Environment.GetEnvironmentVariable("INSTANCE_NAME")} | HIT");
            if (Request.Headers.TryGetValue("X-Client-Id", out var clientId))
            {
                Console.WriteLine($"{Environment.GetEnvironmentVariable("INSTANCE_NAME")} | X-Client-Id: {clientId}");

                //Try rate limit here using redis + rate limiter
                var result = await _limiter.CheckAsync(clientId);
                Console.WriteLine(JsonConvert.SerializeObject(result));

                if (result.Allowed) return Ok();
                
                return StatusCode(509);
            }

            return BadRequest();
        }
    }
}
