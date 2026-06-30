using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Common;
using OrderService.Application.Handler.ReplayProjection;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Services;
using OrderService.Domain.Entities;

namespace OrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReplayController : ControllerBase
    {
        private readonly ReplayProjectionHandler _replayProjectionHandler;

        public ReplayController(ReplayProjectionHandler replayProjectionHandler)
        {
            _replayProjectionHandler = replayProjectionHandler;
        }


        //For learning purpose only serves as API, in production it might be an application/.exe, service, or anything
        [HttpPost("run")]
        public async Task<IActionResult> Replay(CancellationToken cancellationToken)
        {
            await _replayProjectionHandler.HandleAsync(cancellationToken);

            return Ok();
        }
    }
}
