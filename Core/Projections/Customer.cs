using System;

namespace orders.projection.worker.Core.Ports.Repositories.Projections;

public class Customer
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }

}
