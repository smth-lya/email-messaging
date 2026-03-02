using Homework.Api;
using System.Reflection;
using Homework.Api.Database;
using Homework.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCacheProxy(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opts =>
    {
        var file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        opts.IncludeXmlComments(
            Path.Combine(AppContext.BaseDirectory, file));
    });
}

var app = builder.Build();

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