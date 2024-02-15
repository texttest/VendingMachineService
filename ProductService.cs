using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace VendingMachine_Logic;

public class ProductService
{
    private readonly IMongoCollection<Product> _productsCollection;

    public ProductService(
        IOptions<VendingMachineDatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(
            dbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            dbSettings.Value.DatabaseName);

        _productsCollection = mongoDatabase.GetCollection<Product>(
            dbSettings.Value.StockCollectionName);
    }
    public async Task<List<Product>> GetAsync() =>
        await _productsCollection.Find(_ => true).ToListAsync();

    public async Task<Product?> GetAsync(string id) =>
        await _productsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Product newBook) =>
        await _productsCollection.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Product updatedBook) =>
        await _productsCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _productsCollection.DeleteOneAsync(x => x.Id == id);
}