using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Common;
using OrderService.Application.Handler.CreateOrder;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand body)
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
    }
}
