using Microsoft.Extensions.Options;
using MongoDB.Driver;
using orders.customer.dash.worker.Adapters.Infra.Database.Options;
using orders.customer.dash.worker.Core.Events;
using orders.customer.dash.worker.Core.Ports.Repositories;
using orders.customer.dash.worker.Core.Projections;

namespace orders.customer.dash.worker.Adapters.Infra.Repositories;

public class OrderSummaryRepository : ICustomerDashRepository
{
    private readonly IMongoCollection<CustomerDash> _customerDash;
    private readonly IMongoCollection<Customer> _customer;
    public OrderSummaryRepository(IOptions<MongoDbOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _customerDash = database.GetCollection<CustomerDash>(nameof(CustomerDash));
        _customer = database.GetCollection<Customer>(nameof(Customer));
    }
    public async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        var filter = Builders<CustomerDash>.Filter
            .Eq(p => p.Id, @event.CustomerId);
        var sortDefinition = Builders<RecentOrder>.Sort
            .Descending(o => o.Timestamp);

        var customerRef = await _customer.Find(p => p.Id.Equals(@event.CustomerId)).FirstOrDefaultAsync();

        var update = Builders<CustomerDash>.Update
            .Set(p => p.CustomerName, customerRef.CustomerName)
            .Inc(p => p.TotalOrders, 1)
            .PushEach(p => p.RecentOrders, [new RecentOrder()
            {
                OrderId = @event.OrderId,
            }],
            slice: 5,
            sort: sortDefinition);

        await _customerDash.UpdateOneAsync(
            filter: filter,
            update: update,
            options: new UpdateOptions { IsUpsert = true }
        );
    }


    public async Task HandleOrderPaidAsync(OrderPaidEvent @event)
    {
        var filter = Builders<CustomerDash>.Filter
            .Eq(p => p.Id, @event.CustomerId);

        var update = Builders<CustomerDash>.Update
            .Inc(p => p.LifetimeValue, @event.Total);

        await _customerDash.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true }
        );
    }
}
