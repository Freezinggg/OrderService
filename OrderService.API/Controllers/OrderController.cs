using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Common;
using OrderService.Application.DTO;
using OrderService.Application.Handler.CancelOrder;
using OrderService.Application.Handler.CreateOrder;
using OrderService.Application.Handler.GetOrderById;
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
        private readonly IConcurrencyLimiter _concurrencyLimiter;

        public OrderController(IMediator mediator, IPressureGate pressure, IConcurrencyLimiter concurrencyLimiter)
        {
            _mediator = mediator;
            _pressureGate = pressure;
            _concurrencyLimiter = concurrencyLimiter;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand body)
        {
            var context = new PressureContext(
                endpoint: "CreateOrder",
                customerId: null,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            var pressureDecision = _pressureGate.Evaluate(context);
            if (!pressureDecision.IsAllowed)
            {
                if (pressureDecision.RetryAfter.HasValue)
                {
                    Response.Headers["Retry-After"] =
                        ((int)pressureDecision.RetryAfter.Value.TotalSeconds).ToString();
                }
                return StatusCode(429);
            }


            var concurrencyDecision = _concurrencyLimiter.TryAcquire();
            if (!concurrencyDecision.IsAllowed)
            {
                return StatusCode(503, new
                {
                    reason = "capacity_exhausted",
                    retryStrategy = concurrencyDecision.hint?.Strategy,
                    minBackoffMs = concurrencyDecision.hint?.MinDelayMs,
                    maxBackoffMs = concurrencyDecision.hint?.MaxDelayMs
                });
            }
            try
            {
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
            finally
            {
                _concurrencyLimiter.Release();
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetById(Guid orderId)
        {
            var result = await _mediator.Send(new GetOrderByIdQuery(orderId));
            return result.Status switch
            {
                ResultStatus.Success => Ok(ApiResponse<OrderSummaryDTO>.Ok(result.Data)),
                ResultStatus.NotFound => NotFound(ApiResponse<OrderSummaryDTO>.Fail(result.ErrorMessage)),
                _ => StatusCode(500, ApiResponse<Guid>.Fail("Unhandled result status")) //default value if ResultStatus is its new or default
            };
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrdeer(Guid orderId)
        {
            var result = await _mediator.Send(new CancelOrderCommand(orderId));
            return result.Status switch
            {
                ResultStatus.Success => Ok(ApiResponse<bool>.Ok(result.Data)),
                ResultStatus.Invalid => BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage)),
                ResultStatus.Fail => Conflict(ApiResponse<bool>.Fail(result.ErrorMessage)),
                ResultStatus.Error => StatusCode(500, ApiResponse<bool>.Fail(result.ErrorMessage)),
                _ => StatusCode(500, ApiResponse<bool>.Fail("Unhandled result status"))
            };
        }
    }
}
