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
builder.Services.AddSingleton<VendingMachine>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    var captureMock = Environment.GetEnvironmentVariable("CAPTUREMOCK_SERVER");
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");

        if (!string.IsNullOrEmpty(captureMock))
        {
            Console.Out.WriteLine("Setting up interceptor for CaptureMock");
            options.UseRequestInterceptor("(req) => { req.url = req.url.replace(/http:..(127.0.0.1|localhost):[0-9]+/, '" +
                                          captureMock + "'); return req; }");
        }
    });

    app.UseDeveloperExceptionPage();
    app.UseStaticFiles();
}

app.MapGet("/coins", (VendingMachine machine) =>
    {
        return machine.GetCoins();
    })
    .WithName("GetInsertedCoins")
    .WithOpenApi();

app.MapPut("/coins", (VendingMachine machine, int coin) =>
    {
        machine.InsertCoin(coin);
    })
    .WithName("InsertCoin")
    .WithOpenApi();

app.MapGet("/display", (VendingMachine machine) =>
    {
        return machine.DisplayBalance();
    })
    .WithName("GetDisplay")
    .WithOpenApi();


app.MapPost("/empty_machine", (VendingMachine machine) =>
    {
        machine.Empty();
    })
    .WithName("EmptyMachine")
    .WithOpenApi();

app.Run();