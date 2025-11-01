using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using orders.projection.worker.Adapters.Infra.Database.Options;
using orders.projection.worker.Core.Events;
using orders.projection.worker.Core.Ports.Repositories;
using orders.projection.worker.Core.Ports.Repositories.Projections;

namespace orders.projection.worker.Adapters.Infra.Repositories;

public class OrderSummaryRepository : IOrderSummaryRepository
{
    private readonly IMongoCollection<OrderSummary> _collection;
    public OrderSummaryRepository(IOptions<MongoDbOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<OrderSummary>(nameof(OrderSummary));
    }
    public async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        var customerRef = new Customer { CustomerId = @event.CustomerId };

        var projection = new OrderSummary
        {
            OrderId = @event.OrderId,
            Customer = customerRef,
        };

        var filter = Builders<OrderSummary>.Filter
            .Eq(p => p.OrderId, @event.OrderId);

        await _collection.ReplaceOneAsync(
            filter: filter,
            replacement: projection,
            options: new ReplaceOptions { IsUpsert = true }
        );

        var update = Builders<OrderSummary>.Update.Push(p => p.Status, new OrderStatus()
        {
            Status = "Pending",
            Timestamp = @event.Timestamp
        });

        await _collection.UpdateOneAsync(
            filter, 
            update, 
            new UpdateOptions { IsUpsert = true }
        );

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
