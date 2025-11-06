namespace orders.customer.dash.worker.Core.Projections.OrderSummary
{
    public class OrderItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}