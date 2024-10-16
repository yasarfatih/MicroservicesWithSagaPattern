using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Payment.API.Consumers
{
    public class StockReservedRequestPaymentConsumer : IConsumer<IStockReservedRequestPaymentEvent>
    {
        public readonly ILogger<StockReservedRequestPaymentEvent> _logger;
        public readonly IPublishEndpoint _publishEndpoint;
        public StockReservedRequestPaymentConsumer(ILogger<StockReservedRequestPaymentEvent> logger,IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        public async Task Consume(ConsumeContext<IStockReservedRequestPaymentEvent> context)
        {
            var balance = 3000m;

            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was withdrawn from credit card for user id= {context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymentCompletedEvent(context.Message.CorrelationId));
            }
            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not withdrawn from credit card for user id={context.Message.BuyerId}");

                await _publishEndpoint.Publish(new PaymentFailedEvent(context.Message.CorrelationId) { Reason = "not enough balance", OrderItems = context.Message.OrderItems });
            }
        }
    }
}
