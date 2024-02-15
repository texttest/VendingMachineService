namespace VendingMachine_Logic;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class VendingMachineController
{
    private readonly ProductService _productService;
    private readonly BankService _bankService;
    private readonly VendingMachine machine;

    public VendingMachineController(ProductService productService,
        BankService bankService)
    {
        _productService = productService;
        _bankService = bankService;
        machine = new VendingMachine(_bankService, _productService);
    }

    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
        return machine.Display;
    }
}