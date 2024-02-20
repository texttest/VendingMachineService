using System.Globalization;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace VendingMachine_Logic;

public class VendingMachine
{
    private readonly CultureInfo _en_Us_Culture;

    private readonly BankService _bankService;
    
    public VendingMachine(BankService bankService)
    {
        _bankService = bankService;
        _en_Us_Culture = CultureInfo.CreateSpecificCulture("en-US");
    }

    public async Task<List<int>> GetCoins()
    {
        var collection = await _bankService.GetAsync("Coins");
        if (collection == null)
        {
            await _bankService.InitVendingMachineCollections();
            collection = await _bankService.GetAsync("Coins");
        }

        return collection.Coins;
    }
    
    public async Task<int> Balance()
    {
        return GetCoins().Result.Sum();
    }

    public virtual async Task<string> DisplayBalance()
    {
        var balance = await Balance();
        var display = balance != 0 ? FormatAsDollars(balance) : "INSERT COIN";
        return display;
    }
    
    protected string FormatAsDollars(int cents)
    {
        return (cents / 100.0).ToString("C", _en_Us_Culture);
    }

    public virtual async void InsertCoin(int coin)
    {
        await _bankService.AddCoinToCollection(coin, "Coins");
    }

    public async void Empty()
    {
        await _bankService.InitVendingMachineCollections();
    }
}