using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using orders.customer.dash.worker;
using orders.customer.dash.worker.Adapters.Infra.Database.Options;
using orders.customer.dash.worker.Adapters.Infra.Messaging.Options;
using orders.customer.dash.worker.Adapters.Infra.Repositories;
using orders.customer.dash.worker.Core.Ports.Repositories;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddSingleton<ICustomerDashRepository, OrderSummaryRepository>();

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
    var factory = new ConnectionFactory
    {
        HostName = options.HostName,
        UserName = options.UserName,
        Password = options.Password,
        AutomaticRecoveryEnabled = true
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
