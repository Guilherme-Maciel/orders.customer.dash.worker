using System;

namespace orders.projection.worker.Core.Events;

public class OrderItemAddedEvent
{
    public string OrderId { get; set; }
}
