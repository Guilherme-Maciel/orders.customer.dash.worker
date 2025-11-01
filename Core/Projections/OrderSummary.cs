using System;

namespace orders.projection.worker.Core.Ports.Repositories.Projections;

public class OrderSummary
{
    public string OrderId { get; set; }
    public IEnumerable<OrderStatus> Status { get; set; }
    public Customer Customer { get; set; }

}

public class OrderStatus
{
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
}
