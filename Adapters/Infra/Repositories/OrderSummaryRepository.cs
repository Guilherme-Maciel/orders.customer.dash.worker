using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using orders.projection.worker.Adapters.Infra.Database.Options;
using orders.projection.worker.Core.Events;
using orders.projection.worker.Core.Ports.Repositories;
using orders.projection.worker.Core.Ports.Repositories.Projections;
using orders.projection.worker.Core.Projections.OrderSummary;

namespace orders.projection.worker.Adapters.Infra.Repositories;

public class OrderSummaryRepository : IOrderSummaryRepository
{
    private readonly IMongoCollection<OrderSummary> _orderSummary;
    private readonly IMongoCollection<Product> _product;
    private readonly IMongoCollection<Customer> _customer;
    public OrderSummaryRepository(IOptions<MongoDbOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _orderSummary = database.GetCollection<OrderSummary>(nameof(OrderSummary));
        _product = database.GetCollection<Product>(nameof(Product));
        _customer = database.GetCollection<Customer>(nameof(Customer));
    }
    public async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        await CreateOrReplaceOrderSummary(@event.OrderId, @event.CustomerId, async (filter) =>
        {
            var update = Builders<OrderSummary>.Update.Push(p => p.Status, new OrderStatus()
            {
                Status = "Pending",
                Timestamp = @event.Timestamp
            });

            await _orderSummary.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true }
            );
        });

    }

    public async Task HandleOrderItemAddedAsync(OrderItemAddedEvent @event)
    {
        await CreateOrReplaceOrderSummary(@event.OrderId, @event.CustomerId, async (filter) =>
        {
            var product = await _product.Find(p => p.ProductId.Equals(@event.ProductId)).FirstAsync();

            var item = new OrderItem()
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Quantity = @event.Quantity,
                Price = product.Price
            };

            var update = Builders<OrderSummary>.Update.Push(p => p.Items, item);

            await _orderSummary.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true }
            );
        });
    }

    public Task HandleOrderSubmittedAsync(OrderSubmittedEvent @event)
    {
        throw new NotImplementedException();
    }

    private async Task CreateOrReplaceOrderSummary(Guid orderId, Guid customerId, Func<FilterDefinition<OrderSummary>,Task> afterFunc)
    {
        var customerRef = await _customer.Find(p=>p.CustomerId.Equals(customerId)).FirstAsync();

        var projection = new OrderSummary
        {
            OrderId = orderId,
            Customer = customerRef,
        };

        var filter = Builders<OrderSummary>.Filter
            .Eq(p => p.OrderId, orderId);

        await _orderSummary.ReplaceOneAsync(
            filter: filter,
            replacement: projection,
            options: new ReplaceOptions { IsUpsert = true }
        );

        await afterFunc(filter);
    }
}
