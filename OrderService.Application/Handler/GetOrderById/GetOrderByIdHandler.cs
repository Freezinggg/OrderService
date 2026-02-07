using MediatR;
using OrderService.Application.Common;
using OrderService.Application.DTO;
using OrderService.Application.Interface;
using OrderService.Application.Interface.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.GetOrderById
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderSummaryDTO>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderSummaryCache _cache;

        public GetOrderByIdHandler(IOrderRepository orderRepository, IOrderSummaryCache cache)
        {
            _orderRepository = orderRepository;
            _cache = cache;
        }

        public async Task<Result<OrderSummaryDTO>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            //Check cache first, if exist take cached val. if not then query from db
            OrderSummaryDTO? cachedDto = await _cache.GetAsync(request.Id, cancellationToken);
            if(cachedDto != null) return Result<OrderSummaryDTO>.Success(cachedDto);

            var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
            if(order != null)
            {
                OrderSummaryDTO dto = new(
                    order.Id,
                    order.CustomerId,
                    order.Status,
                    order.CreatedAt,
                    order.ExpiredAt,
                    order.Items.Select(x => new OrderItemDTO(x.Code, x.Quantity)).ToList().AsReadOnly());

                //Set cache value
                await _cache.SetAsync(request.Id, dto, cancellationToken);
                return Result<OrderSummaryDTO>.Success(dto);
            }

            return Result<OrderSummaryDTO>.NotFound("Order doesnt exist.");
        }
    }
}
