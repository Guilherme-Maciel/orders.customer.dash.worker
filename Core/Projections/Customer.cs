using MongoDB.Bson.Serialization.Attributes;
using System;

namespace orders.customer.dash.worker.Core.Projections;
[BsonIgnoreExtraElements]
public class Customer
{
    [BsonId]
    [BsonElement("_id")]
    public Guid Id { get; set; }
    public string CustomerName { get; set; }

}
