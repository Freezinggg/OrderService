using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Common;
using OrderService.Application.Handler.CreateOrder;
using OrderService.Application.Interface;
using OrderService.Application.RateLimit;
using OrderService.Infrastructure.Pressure;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPressureGate _pressureGate;
        private readonly IOrderMetric _metric;
        private readonly IPressureMetric _pressureMetric;

        public OrderController(IMediator mediator, IPressureGate pressure, IOrderMetric metric, IPressureMetric pressureMetric)
        {
            _mediator = mediator;
            _pressureGate = pressure;
            _metric = metric;
            _pressureMetric = pressureMetric;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand body)
        {
            var context = new PressureContext(
                endpoint: "CreateOrder",
                customerId: null,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            var decision = _pressureGate.Evaluate(context);


            if (!decision.IsAllowed)
            {
                if (decision.RetryAfter.HasValue)
                {
                    Response.Headers["Retry-After"] =
                        ((int)decision.RetryAfter.Value.TotalSeconds).ToString();
                }

                _pressureMetric.RecordRejected();

                return StatusCode(429);
            }

            _pressureMetric.RecordAllowed();

            var result = await _mediator.Send(body);
            //return result.IsSuccess ? Ok(ApiResponse<Guid>.Ok(result.Data)) : BadRequest(ApiResponse<Guid>.Fail(result.ErrorMessage));

            return result.Status switch
            {
                ResultStatus.Success => Ok(ApiResponse<Guid>.Ok(result.Data)),
                ResultStatus.Invalid => BadRequest(ApiResponse<Guid>.Fail(result.ErrorMessage)),
                ResultStatus.Fail => Conflict(ApiResponse<Guid>.Fail(result.ErrorMessage)),
                ResultStatus.Error => StatusCode(500, ApiResponse<Guid>.Fail(result.ErrorMessage)),
                _ => StatusCode(500, ApiResponse<Guid>.Fail("Unhandled result status")) //default value if ResultStatus is its new or default
            };
        }
    }
}
