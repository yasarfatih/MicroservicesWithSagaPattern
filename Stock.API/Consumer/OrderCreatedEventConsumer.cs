using MassTransit;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using Stock.API.Model;

namespace Stock.API.Consumer
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
    {
        private readonly AppDbContext _context;
        private ILogger<OrderCreatedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpointProvider;

        public OrderCreatedEventConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpointProvider = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _context.Stock.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }
            if (stockResult.All(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _context.Stock.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);
                    if (stock != null)
                    {
                        stock.Count -= item.Count;
                    }
                    await _context.SaveChangesAsync();
                }
                _logger.LogInformation($"Stock reserved for CorrelationId:{context.Message.CorrelationId}");
                StockReservedEvent stockReservedEvent = new StockReservedEvent(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems,
                };
                await _publishEndpointProvider.Publish(stockReservedEvent);
            }
            else
            {
                await _publishEndpointProvider.Publish(new StockNotReservedEvent(context.Message.CorrelationId)
                {
                    Reason = "Not Enough stock"

                });
                _logger.LogInformation($"Stock not reserved for CorrelationId:{context.Message.CorrelationId}");
            }
        }
    }
}
