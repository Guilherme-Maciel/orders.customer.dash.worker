using System;

namespace orders.projection.worker.Core.Events;

public class OrderCreatedEvent
{
    public string OrderId { get; set; }
}
