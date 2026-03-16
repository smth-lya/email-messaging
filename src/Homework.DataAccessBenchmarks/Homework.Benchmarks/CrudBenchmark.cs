using BenchmarkDotNet.Attributes;
using Dapper;
using Homework.Benchmarks.EfCore;
using Homework.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Homework.Benchmarks;

[MemoryDiagnoser]
[KeepBenchmarkFiles]
public class CrudBenchmark
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
        await _categoryGenerator.GenerateCategories(200);
        
        _productsGenerator = new ProductsGenerator(_connectionString, _random, _categoryGenerator.GetCategoryIds().ToList());
        await _productsGenerator.GenerateProducts(10000);

        _testProductId = _productsGenerator.GetRandomProductId();
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
        await _databaseManager.DropDatabaseAsync();
    }
}