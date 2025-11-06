using System;

namespace orders.customer.dash.worker.Adapters.Infra.Database.Options;

public class MongoDbOptions
{
    public string DatabaseName { get; set; }
    public string ConnectionString { get; set; }
}
