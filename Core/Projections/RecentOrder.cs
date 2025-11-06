namespace orders.customer.dash.worker.Core.Projections
{
    public class RecentOrder
    {
        public Guid OrderId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
