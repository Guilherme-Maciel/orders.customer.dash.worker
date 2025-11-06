using System;
using orders.customer.dash.worker.Core.Events;

namespace orders.customer.dash.worker.Core.Ports.Repositories;

public interface ICustomerDashRepository
{
    Task HandleOrderCreatedAsync(OrderCreatedEvent @event);
    Task HandleOrderPaidAsync(OrderPaidEvent @event);
}
