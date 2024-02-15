namespace VendingMachine_Logic;

public class VendingMachineDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string StockCollectionName { get; set; } = null!;
    public string BankCollectionName { get; set; } = null!;
}