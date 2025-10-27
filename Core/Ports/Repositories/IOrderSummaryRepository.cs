using System;
using orders.projection.worker.Core.Events;

namespace orders.projection.worker.Core.Ports.Repositories;

public interface IOrderSummaryRepository
{
    Task HandleOrderCreatedAsync(OrderCreatedEvent @event);
    Task HandleOrderItemAddedAsync(OrderItemAddedEvent @event);
    Task HandleOrderSubmittedAsync(OrderSubmittedEvent @event);
}
