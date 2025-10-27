using System;

namespace orders.projection.worker.Adapters.Infra.Messaging.Options;

public class RabbitMqOptions
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string QueueName { get; set; }
}
