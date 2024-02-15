using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace VendingMachine_Logic;

public class BankService
{
    private readonly IMongoCollection<Bank> _banks;

    public BankService(
        IOptions<VendingMachineDatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(
            dbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            dbSettings.Value.DatabaseName);

        _banks = mongoDatabase.GetCollection<Bank>(
            dbSettings.Value.BankCollectionName);
    }
    public async Task<List<Bank>> GetAsync() =>
        await _banks.Find(_ => true).ToListAsync();

    public async Task<Bank?> GetAsync(string id) =>
        await _banks.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Bank newBook) =>
        await _banks.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Bank updatedBook) =>
        await _banks.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _banks.DeleteOneAsync(x => x.Id == id);
}