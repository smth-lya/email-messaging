using Homework.Api;
using System.Reflection;
using Homework.Api.Database;
using Homework.Api.Logging;
using Homework.Api.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCacheProxy(builder.Configuration);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Host.UseSerilog((context, services, config) => config
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CacheProxy")
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] CorrelationId={CorrelationId} {Message:lj}{NewLine}{Exception}")
);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opts =>
    {
        // var file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        // opts.IncludeXmlComments(
        //     Path.Combine(AppContext.BaseDirectory, file));
    });
}

var app = builder.Build();

app.UseStructuredLogging();

app.MapPost("/product/create", async (CreateProduct product, IProductRepository repository) =>
    await repository.AddAsync(new Product(Guid.NewGuid(), product.Name, product.Price)));

app.MapGet("/product/{id:guid}", async (Guid id, IProductRepository repository) => 
    await repository.GetAsync(id));

app.MapPost("/product/update", async (Product product, IProductRepository repository) => 
    await repository.UpdateAsync(product));

app.MapPost("/product/delete/{id:guid}", async (Guid id, IProductRepository repository) => 
    await repository.DeleteAsync(id));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    MigrateDatabase(app);
}

app.Run();
return;

static void MigrateDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
    
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}