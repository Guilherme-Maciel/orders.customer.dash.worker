using System;

namespace orders.customer.dash.worker.Core.Projections;

public class Product
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
}
