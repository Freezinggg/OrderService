using MediatR;
using OrderService.Application.Common;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<Result<OrderSummaryDTO>>
    {
        public Guid Id { get; }
        public Guid CustomerId { get; set; }
        public GetOrderByIdQuery(Guid id) => Id = id;
        
    }
}
