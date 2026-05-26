using MediatR;
using Newtonsoft.Json;
using OrderService.Application.Common;
using OrderService.Application.DTO;
using OrderService.Application.Interface.Cache;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Outbox.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.GetOrderById
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderSummaryDTO>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderProjectionRepository _orderProjectionRepository;
        private readonly IOrderSummaryCache _cache;

        public GetOrderByIdHandler(IOrderRepository orderRepository, IOrderProjectionRepository orderProjectionRepository, IOrderSummaryCache cache)
        {
            _orderRepository = orderRepository;
            _orderProjectionRepository = orderProjectionRepository;
            _cache = cache;
        }

        public async Task<Result<OrderSummaryDTO>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            //Check cache first, if exist take cached val. if not then query from db
            OrderSummaryDTO? cachedDto = await _cache.GetAsync(request.Id, cancellationToken);
            if(cachedDto != null) return Result<OrderSummaryDTO>.Success(cachedDto);

            var orderProjection = await _orderProjectionRepository.GetByIdAsync(request.Id, cancellationToken);
            if(orderProjection is not null)
            {
                var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
                if (order != null)
                {
                    OrderSummaryDTO dto = new(
                        orderProjection.OrderId,
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

            return Result<OrderSummaryDTO>.NotFound("Projection doesnt exist.");
        }
    }
}
