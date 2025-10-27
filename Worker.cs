using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using orders.projection.worker.Adapters.Infra.Messaging.Options;
using orders.projection.worker.Core.Events;
using orders.projection.worker.Core.Ports.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace orders.projection.worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _rabbitConnection;
    private readonly string _queueName;
    private IChannel _channel;
    private readonly IOrderSummaryRepository _repository;

    public Worker(ILogger<Worker> logger, IConnection rabbitConnection, IOptions<RabbitMqOptions> options, IOrderSummaryRepository repository)
    {
        _logger = logger;
        _rabbitConnection = rabbitConnection;
        _queueName = options.Value.QueueName;
        _repository = repository;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = await _rabbitConnection.CreateChannelAsync();

        // Garante que a fila existe (como no definitions.json)
        await _channel.QueueDeclareAsync(queue: _queueName,
                              durable: true,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        // Define Quality of Service: processa 1 mensagem por vez.
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
                // Processa a mensagem
                await HandleMessageAsync(message);

                // Confirma ao RabbitMQ que a mensagem foi processada com sucesso
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem: {Message}", message);

                // Rejeita a mensagem e a devolve para a fila para nova tentativa
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);

                // (Em produção, você teria uma política de "dead-letter queue" 
                // para mensagens que falham repetidamente)
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName,
                        autoAck: false, // Nós faremos o ACK manual
                        consumer: consumer);

        await Task.CompletedTask;

    }
    
    private async Task HandleMessageAsync(string message)
    {
        // 1. "Espia" o evento para descobrir o tipo
        var baseEvent = JsonSerializer.Deserialize<EventBase>(message);

        // 2. Roteia para o handler correto
        switch (baseEvent?.EventType)
        {
            case "order_created":
                var createEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                _logger.LogInformation("Processando OrderCreatedEvent: {OrderId}", createEvent.OrderId);
                await _repository.HandleOrderCreatedAsync(createEvent);
                break;
            
            case "order_item_added":
                var itemAddedEvent = JsonSerializer.Deserialize<OrderItemAddedEvent>(message);
                _logger.LogInformation("Processando OrderItemAddedEvent para: {OrderId}", itemAddedEvent.OrderId);
                await _repository.HandleOrderItemAddedAsync(itemAddedEvent);
                break;
            
            // case "order_submitted": ...

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
