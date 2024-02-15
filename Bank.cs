using MongoDB.Bson.Serialization.Attributes;

namespace VendingMachine_Logic;

using MongoDB.Bson;

public class Bank
{
    [BsonId]
    [BsonRepresentation((BsonType.ObjectId))]
    public string? Id { get; set; }

    [BsonElement("Name")] public string CoinCollectionName { get; set; }
    
    public List<int> Coins { get; set; }
}