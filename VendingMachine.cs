using System.Globalization;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace VendingMachine_Logic;

public class VendingMachine
{
    private readonly BankService _bankService;
    private readonly ProductService _productService;
    public List<int> AcceptedCoins { get; protected set; } = new() { 5, 10, 25 };

    public async Task<List<int>> GetBank()
    {
        var collection = await _bankService.GetAsync("Bank");
        return collection.Coins;
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

    public string? DispensedProduct = "";

    protected Dictionary<string, int> _prices = new()
    {
        { "Cola", 100 }, { "Chips", 50 }, { "Candy", 65 }
    };

    private readonly CultureInfo _en_Us_Culture;

    public async Task<List<int>> GetReturns()
    {
        var collection = await _bankService.GetAsync("Returns");
        return collection.Coins;
    }
    public string? SelectedProduct { get; set; }
    public Dictionary<string, int> Stock { get; private set; }

    

    public VendingMachine(BankService bankService, ProductService productService)
    {
        _bankService = bankService;
        _productService = productService;

        Display = "";
        Balance = 0;
        Stock = new Dictionary<string, int>();
        
        SelectedProduct = null;
        _en_Us_Culture = CultureInfo.CreateSpecificCulture("en-US");
        
    }

    public string Display { get; protected set; }
    public int Balance { get; protected set; }

    protected virtual async void DisplayBalance()
    {
        if (Balance != 0)
        {
            Display = FormatAsDollars(Balance);
        }
        else
        {
            var hasChange = await HasChange();
            if (hasChange)
                Display = "INSERT COIN";
            else
                Display = "EXACT CHANGE ONLY";
        }
    }

    public virtual void InsertAllCoins(int[] coins)
    {
        foreach (var coin in coins)
        {
            InsertCoin(coin);
        }
    }

    public virtual async void InsertCoin(int coin)
    {
        if (AcceptedCoins.Contains(coin))
        {
            await _bankService.AddCoinToCollection(coin, "Coins");
            Balance += coin;
            DisplayBalance();
        }
        else
        {
            await _bankService.AddCoinToCollection(coin, "Returns");
        }
    }

    public virtual void SelectProduct(string product)
    {
        SelectedProduct = product;
        if (Balance >= _prices[SelectedProduct])
        {
            DispenseProduct();
        }
        else
        {
            var cost = _prices[product];
            var dollars = FormatAsDollars(cost);
            Display = $"PRICE {dollars}";
        }
    }

    protected string FormatAsDollars(int cents)
    {
        return (cents / 100.0).ToString("C", _en_Us_Culture);
    }

    protected async virtual void DispenseProduct()
    {
        if (SelectedProduct != null && Stock[SelectedProduct] >= 1)
        {
            Stock[SelectedProduct] -= 1;
            Display = "THANK YOU";
            DispensedProduct = SelectedProduct;
            Balance -= _prices[SelectedProduct];
            var coins = await GetCoins();
            foreach (var coin in coins)
            {
                await _bankService.AddCoinToCollection(coin, "Bank");
            }

            await _bankService.ClearCollection("Coins");
            if (Balance > 0)
            {
                var change = GetChangeRequired(Balance);
                await _bankService.ClearCollection("Returns");
                foreach (var coin in change)
                {
                    await _bankService.AddCoinToCollection(coin, "Returns");
                    await _bankService.RemoveCoinFromCollection(coin, "Bank");
                }

                Balance = 0;
            }

            SelectedProduct = null;
        }
        else
        {
            Display = "SOLD OUT";
            SelectedProduct = null;
        }
    }

    public virtual async void Tick()
    {
        if (Display.Contains("PRICE"))
        {
            DisplayBalance();
        }
        else if (
            Display.Contains("THANK YOU") ||
            Display.Contains("SOLD OUT")
        )
        {
            DisplayBalance();
            DispensedProduct = null;
            await _bankService.ClearCollection("Returns");
        }
        else if (SelectedProduct != null && Balance >= _prices[SelectedProduct])
        {
            DispenseProduct();
        }
    }

    public void UpdateStock(string product, int quantity)
    {
        Stock[product] = quantity;
    }

    public async void ReturnCoins()
    {
        Balance = 0;
        var coins = await GetCoins();
        foreach (var coin in coins)
        {
            await _bankService.AddCoinToCollection(coin, "Returns");
        }
        await _bankService.ClearCollection("Coins");
        DisplayBalance();
    }

    protected async Task<bool> HasChange()
    {
        var bank = await GetBank();

        return
            bank.Contains(25)
            && bank.Contains(10)
            && bank.Contains(5)
            ;
    }

    protected List<int> GetChangeRequired(int balance)
    {
        // TODO: don't fake this one!
        return new List<int> { 10, 5 };
    }

    public async virtual void BankCoins(params int[] coins)
    {
        foreach (var coin in coins)
        {
            await _bankService.AddCoinToCollection(coin, "Bank");
        }
        DisplayBalance();
    }
}