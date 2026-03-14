using BenchmarkDotNet.Attributes;
using Dapper;
using Homework.Benchmarks.EfCore;
using Homework.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Homework.Benchmarks;

[MemoryDiagnoser]
[KeepBenchmarkFiles]
public class CrudBenchmark : IAsyncDisposable
{
    private const string MasterConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";
    
    private readonly Random _random = new(100);

    private DatabaseManager _databaseManager = null!;
    private CategoriesGenerator _categoryGenerator = null!;
    private ProductsGenerator _productsGenerator = null!;

    private Guid _testProductId;
    private string _connectionString = null!;
    
    [GlobalSetup]
    public async Task Setup()
    {
        _databaseManager = new DatabaseManager(MasterConnectionString, _random);

        await _databaseManager.CreateDatabaseAsync();
        await _databaseManager.CreateSchemaAsync();
        
        _connectionString = _databaseManager.ConnectionString;

        _categoryGenerator = new CategoriesGenerator(_connectionString, _random);
        await _categoryGenerator.GenerateCategories(20);
        
        _productsGenerator = new ProductsGenerator(_connectionString, _random, _categoryGenerator.GetCategoryIds().ToList());
        await _productsGenerator.GenerateProducts(1000);

        _testProductId = _productsGenerator.GetRandomProductId();
    }

    private async Task WarmUpAsync()
    {
        await using var efContext = new ApplicationDbContext(_connectionString);
        await efContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == _testProductId);
        
        await using var dapperConnection = new NpgsqlConnection(_connectionString);
        await dapperConnection.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE ProductId = @Id LIMIT 1", 
            new { Id = _testProductId });
        
        await using var statConnection = new NpgsqlConnection(_connectionString);
        await statConnection.ExecuteAsync("ANALYZE Products; ANALYZE Categories;");
    }
    
    [Benchmark(Baseline = true)]
    public async Task<Product?> EF_GetFirst()
    {
        await using var context = new ApplicationDbContext(_connectionString);
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.ProductId == _testProductId);
    }

    [Benchmark]
    public async Task<Product?> Dapper_GetFirst()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection
            .QueryFirstOrDefaultAsync<Product>(
                "SELECT * FROM Products WHERE ProductId=@Id LIMIT 1", 
                new { Id = _testProductId });
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _productsGenerator.CleanupProducts();
        await _categoryGenerator.CleanupCategories();
    }

    public async ValueTask DisposeAsync()
    {
        await _databaseManager.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}