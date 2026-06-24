using MediatR;
using OrderService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.CompleteOrder
{
    public class CompleteOrderCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public CompleteOrderCommand(Guid id) => Id = id;
    }
}
