using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using orders.customer.dash.worker.Adapters.Infra.Messaging.Options;
using orders.customer.dash.worker.Core.Events;
using orders.customer.dash.worker.Core.Ports.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace orders.customer.dash.worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _rabbitConnection;
    private readonly string _queueName;
    private IChannel _channel;
    private readonly ICustomerDashRepository _repository;

    public Worker(ILogger<Worker> logger, IConnection rabbitConnection, IOptions<RabbitMqOptions> options, ICustomerDashRepository repository)
    {
        _logger = logger;
        _rabbitConnection = rabbitConnection;
        _queueName = options.Value.QueueName;
        _repository = repository;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = await _rabbitConnection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: _queueName,
                              durable: true,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation("Worker pronto, aguardando mensagens na fila: {QueueName}", _queueName);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                await HandleMessageAsync(message);

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem: {Message}", message);

                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName,
                        autoAck: false,
                        consumer: consumer);

        await Task.CompletedTask;

    }
    
    private async Task HandleMessageAsync(string message)
    {
        var baseEvent = JsonSerializer.Deserialize<EventBase>(message);
        if (baseEvent is null)
            throw new Exception("Base Event was null");

        switch (baseEvent.EventType)
        {
            case "order_created":
                var createEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                _logger.LogInformation("Processando OrderCreatedEvent: {OrderId}", createEvent.OrderId);
                await _repository.HandleOrderCreatedAsync(createEvent);
                break;

            case "order_item_added":
                break;

            case "order_paid":
                var paidOrder = JsonSerializer.Deserialize<OrderPaidEvent>(message);
                _logger.LogInformation("Processando OrderItemAddedEvent para: {OrderId}", paidOrder.OrderId);
                await _repository.HandleOrderPaidAsync(paidOrder);
                break;
            default:
                _logger.LogWarning("Tipo de evento desconhecido: {EventType}", baseEvent?.EventType);
                break;
        }
    }

    public override void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}
