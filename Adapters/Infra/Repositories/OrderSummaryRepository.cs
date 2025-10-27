using System;
using orders.projection.worker.Core.Events;
using orders.projection.worker.Core.Ports.Repositories;

namespace orders.projection.worker.Adapters.Infra.Repositories;

public class OrderSummaryRepository : IOrderSummaryRepository
{
    public Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        throw new NotImplementedException();
    }

    public Task HandleOrderItemAddedAsync(OrderItemAddedEvent @event)
    {
        throw new NotImplementedException();
    }

    public Task HandleOrderSubmittedAsync(OrderSubmittedEvent @event)
    {
        throw new NotImplementedException();
    }
}
