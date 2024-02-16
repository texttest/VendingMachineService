using MongoDB.Driver;
using VendingMachine_Logic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appconfig = builder.Configuration.GetSection("VendingMachineDatabase");

Console.Out.WriteLine("using mongo connection string " + appconfig.GetValue<string>("ConnectionString"));
builder.Services.Configure<VendingMachineDatabaseSettings>(appconfig);


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB services
builder.Services.AddSingleton<BankService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<VendingMachine>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseStaticFiles();
}

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "docs";

    options.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");

    var captureMock = Environment.GetEnvironmentVariable("SWAGGER_CAPTUREMOCK");
    if (!string.IsNullOrEmpty(captureMock))
    {
        options.UseRequestInterceptor("(req) => { req.url = req.url.replace(/http:..localhost:[0-9]+/, '" +
                                      captureMock + "'); return req; }");
    }
});


string selectedProduct = null;
var stock = new Dictionary<string, int> { { "Chips", 10 }, { "Candy", 10 }, { "Cola", 10 } };


app.MapGet("/stock", (VendingMachine machine) => { return machine.Stock; })
    .WithName("GetStock")
    .WithOpenApi();

app.MapPut("/stock/{name}",
        (VendingMachine machine, string name, int quantity) => { machine.UpdateStock(name, quantity); })
    .WithName("UpdateStock")
    .WithOpenApi();

app.MapGet("/bank", (VendingMachine machine) => { return machine.GetBank(); })
    .WithName("GetBankedCoins")
    .WithOpenApi();

app.MapGet("/coins", (VendingMachine machine) =>
    {
        Console.Out.WriteLine("call to get coins");
        return machine.GetCoins();
    })
    .WithName("GetInsertedCoins")
    .WithOpenApi();

app.MapPost("/coins", (VendingMachine machine, int coin) =>
    {
        machine.InsertCoin(coin);
        machine.Tick();
    })
    .WithName("InsertCoin")
    .WithOpenApi();

app.MapGet("/returns", (VendingMachine machine) => { return machine.GetReturns(); })
    .WithName("GetReturnedCoins")
    .WithOpenApi();

app.MapGet("/display", (VendingMachine machine) => { return machine.Display; })
    .WithName("GetDisplay")
    .WithOpenApi();

app.MapGet("/dispenser", (VendingMachine machine) => { return machine.DispensedProduct; })
    .WithName("GetDispenserContents")
    .WithOpenApi();

app.MapPost("/selectProduct", (VendingMachine machine, string product) =>
    {
        machine.SelectProduct(product);
        machine.Tick();
    })
    .WithName("SelectProduct")
    .WithOpenApi();

app.MapPost("/tick", (VendingMachine machine) => { machine.Tick(); })
    .WithName("Tick")
    .WithOpenApi();

app.Run();