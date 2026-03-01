using Homework.Api;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>();

builder.Services.Configure<ProductCacheSettings>(
    builder.Configuration.GetSection(nameof(ProductCacheSettings)));



// if (builder.Environment.IsDevelopment())
// {
//     builder.Services.AddEndpointsApiExplorer();
//     builder.Services.AddSwaggerGen(opts =>
//     {
//         var file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//         opts.IncludeXmlComments(
//             Path.Combine(AppContext.BaseDirectory, file));
//     });
// }

var app = builder.Build();

app.MapGet("/product/{id:guid}", async (Guid id, IProductRepository repository) => 
    await repository.GetAsync(id, CancellationToken.None));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
    
