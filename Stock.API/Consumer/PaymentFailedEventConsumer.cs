using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Model;

namespace Stock.API.Consumer
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private ILogger<PaymentFailedEventConsumer> _logger;


        public PaymentFailedEventConsumer(AppDbContext context, ILogger<PaymentFailedEventConsumer> logger)
        {
            _appDbContext = context;
            _logger = logger;

        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await _appDbContext.Stock.FirstOrDefaultAsync(x => x.ProductId == orderItem.ProductId);
                if (stock != null)
                {
                    stock.Count += orderItem.Count;
                }
                await _appDbContext.SaveChangesAsync();
            }
            _logger.LogInformation($"Stock was released for Order Id ({context.Message.OrderId})");
        }
    }
}
