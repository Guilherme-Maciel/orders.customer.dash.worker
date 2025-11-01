using System;

namespace orders.projection.worker.Core.Events;

public class OrderCreatedEvent
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public DateTime Timestamp { get; set; }

}
