using System;
using orders.customer.dash.worker.Core.Projections;

namespace orders.customer.dash.worker.Core.Projections.OrderSummary;

public class OrderSummary
{
    public Guid OrderId { get; set; }
    public IEnumerable<OrderStatus> Status { get; set; }
    public Customer Customer { get; set; }
    public IEnumerable<OrderItem> Items { get; set; }

}

public class OrderStatus
{
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
}
