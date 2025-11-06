using System;

namespace orders.customer.dash.worker.Core.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime Timestamp { get; set; }

}
