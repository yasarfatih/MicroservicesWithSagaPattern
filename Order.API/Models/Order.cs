using System.Diagnostics.CodeAnalysis;

namespace Order.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        [NotNull]
        public DateTime CreatedDate { get; set; }
        [NotNull]
        public string BuyerId { get; set; }
        [NotNull]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public OrderStatus? OrderStatus { get; set; }
        
        public string? FailMessage { get; set; }
        [NotNull]
        public Address Address { get; set; }
    }

    public enum OrderStatus
    {
        Suspend,
        Completed,
        Fail
    }
}
