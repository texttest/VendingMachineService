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

    public async Task InitVendingMachineCollections()
    {
        var coins = await GetAsync("Coins");
        if (coins == null)
        {
            Console.Out.WriteLine("creating collection for Coins");
            await CreateAsync(new Bank("Coins") );
        }

        var bank = await GetAsync("Bank");
        if (bank == null)
        {
            Console.Out.WriteLine("creating collection for Bank");
            await CreateAsync(new Bank("Bank") );
        }

        var returns = await GetAsync("Returns");
        if (returns == null)
        {
            Console.Out.WriteLine("creating collection for Returns");
            await CreateAsync(new Bank("Returns"));
        }
        
        coins.Coins.Clear();
        bank.Coins.Clear();
        returns.Coins.Clear();
        await UpdateAsync(coins.Id, coins);
        await UpdateAsync(bank.Id, bank);
        await UpdateAsync(returns.Id, returns);
    }
    
    public async Task AddCoinToCollection(int coin, string collection)
    {
        var bank = await GetAsync(collection);
        if (bank != null)
        {
            bank.Coins.Add(coin);
            await UpdateAsync(bank.Id, bank);
        }
        else
            throw new Exception("Internal error: no bank with name " + collection);
    }    
    
    private string format(List<int> value)
    {
        return "[" + string.Join(", ", value) + "]";
    }
    
    public async Task ClearCollection(string collection)
    {
        var coins = await GetAsync(collection);
        if (coins == null)
            return;
        coins.Coins = new List<int>();
        await UpdateAsync(coins.Id, coins);
    }   
    
    public async Task RemoveCoinFromCollection(int coin, string name)
    {
        var coins = await GetAsync(name);
        coins?.Coins.Remove(coin);
        await UpdateAsync(coins.Id, coins);
    }

    public async Task<List<Bank>> GetAsync() =>
        await _banks.Find(_ => true).ToListAsync();

    public async Task<Bank?> GetAsync(string name) =>
        await _banks.Find(x => x.CoinCollectionName == name).FirstOrDefaultAsync();

    public async Task CreateAsync(Bank newBank) =>
        await _banks.InsertOneAsync(newBank);

    public async Task UpdateAsync(string id, Bank updatedBank) =>
        await _banks.ReplaceOneAsync(x => x.Id == id, updatedBank);

    public async Task RemoveAsync(string id) =>
        await _banks.DeleteOneAsync(x => x.Id == id);

}