using Homework.EfCoreInAction.Database;
using Homework.EfCoreInAction.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/book", async (ApplicationDbContext db) =>
{
    var lastAdded = await db.Books
        .AsNoTracking()
        .OrderByDescending(b => b.Id)
        .FirstOrDefaultAsync();
    
    await db.Books.AddAsync(new Book()
    {
        Id = lastAdded is null ? 1 : lastAdded.Id + 1,
        Title = "C#",
        Description = "C# Description",
    });
    
    await db.SaveChangesAsync();
    
    return Results.Ok("Book Created");
});

if (app.Environment.IsDevelopment())
    MigrateDatabase(app);

app.Run();
return;

static void MigrateDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (context.Database.GetPendingMigrations().Any())
        context.Database.Migrate();
}
