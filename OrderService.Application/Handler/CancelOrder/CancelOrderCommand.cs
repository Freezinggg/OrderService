using MediatR;
using OrderService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.CancelOrder
{
    public class CancelOrderCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public CancelOrderCommand(Guid id) => Id = id;
    }
}
