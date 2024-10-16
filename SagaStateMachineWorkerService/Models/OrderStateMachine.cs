using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);


            Event(() => OrderCreatedRequestEvent, y => y.CorrelateBy<int>(x => x.OrderId, z => z.Message.OrderId).SelectId(context => Guid.NewGuid()));

            Event(() => StockReservedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));


            Initially(When(OrderCreatedRequestEvent).Then(context =>
            {
                context.Saga.BuyerId = context.Message.BuyerId;
                context.Saga.OrderId = context.Message.OrderId;
                context.Saga.CreatedDate = DateTime.Now;
                context.Saga.Cardname = context.Message.Payment.Cardname;
                context.Saga.CardNumber = context.Message.Payment.CardNumber;
                context.Saga.CVV = context.Message.Payment.CVV;
                context.Saga.Expiration = context.Message.Payment.Expiration;
                context.Saga.TotalPrice = context.Message.Payment.TotalPrice;

            }).TransitionTo(OrderCreated).Publish(context => new OrderCreatedEvent(context.Saga.CorrelationId)
            {
                OrderItems = context.Message.OrderItems
            }));

            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettingsConst.PaymentStockReservedRequestQueueName}"), context => new StockReservedRequestPaymentEvent(context.Saga.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems,
                    Payment = new PaymentMessage()
                    {
                        Cardname = context.Saga.Cardname,
                        CardNumber = context.Saga.CardNumber,
                        CVV = context.Saga.CVV,
                        Expiration = context.Saga.CVV,
                        TotalPrice = context.Saga.TotalPrice
                    },
                    BuyerId= context.Saga.BuyerId,
                }
                ).Then(context => { Console.WriteLine($"OrderCreatedRequestEvent After:{context.Saga}"); }));

        }
    }
}