namespace orders.customer.dash.worker.Core.Projections
{
    public class CustomerDash
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal LifetimeValue { get; set; }
        public IEnumerable<RecentOrder> RecentOrders { get; set; }
    }
}
