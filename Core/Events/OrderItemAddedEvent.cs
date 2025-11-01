using System;

namespace orders.projection.worker.Core.Events;

public class OrderItemAddedEvent
{
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public DateTime Timestamp { get; set; }
}
