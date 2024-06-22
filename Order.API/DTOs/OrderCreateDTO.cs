namespace Order.API.DTOs
{
    public class OrderCreateDTO
    {
        public string BuyerId { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
        public PaymentDTO Payment { get; set; }
        public AdressDTO Adress { get; set; }

    }
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }

    public class PaymentDTO
    {
        public string Cardname { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
    }

    public class AdressDTO
    {
        public string Line { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
    }
}
