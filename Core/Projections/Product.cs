using System;

namespace orders.projection.worker.Core.Ports.Repositories.Projections;

public class Product
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
}
