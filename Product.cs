using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VendingMachine_Logic;

public class Product
{
    [BsonId]
    [BsonRepresentation((BsonType.ObjectId))]
    public string? Id { get; set; }

    [BsonElement("Name")] public string ProductName { get; set; }
    public int Quantity { get; set; }
}