using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using System.Diagnostics;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext appDbContext,IPublishEndpoint publishEndpoint)
        {
            _appDbContext = appDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDTO orderCreate)
        {
            var newOrder = new Models.Order()
            {
                BuyerId = orderCreate.BuyerId,
                OrderStatus = OrderStatus.Suspend,
                Address = new Address
                {
                    District = orderCreate.Adress.District,
                    Line = orderCreate.Adress.Line,
                    Province = orderCreate.Adress.Province,
                },
                CreatedDate = DateTime.Now

            };

            foreach (var orderItem in orderCreate.OrderItems)
            {
                newOrder.OrderItems.Add(new OrderItem { Price = orderItem.Price, Count = orderItem.Count, ProductId = orderItem.ProductId });
            }

            await _appDbContext.AddAsync(newOrder);
            await _appDbContext.SaveChangesAsync();

            var OrderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    Cardname = orderCreate.Payment.Cardname,
                    CVV = orderCreate.Payment.CVV,
                    Expiration = orderCreate.Payment.Expiration,
                    CardNumber = orderCreate.Payment.CardNumber,
                    TotalPrice = orderCreate.OrderItems.Sum(x => x.Price * x.Count)
                }
            };

            foreach (var item in orderCreate.OrderItems)
            {
                OrderCreatedEvent.OrderItems.Add(new OrderItemMessage
                {
                    Count = item.Count,
                    ProductId = item.ProductId,
                });
            }

            _publishEndpoint.Publish(OrderCreatedEvent);
            return Ok();
        }
    }
}
