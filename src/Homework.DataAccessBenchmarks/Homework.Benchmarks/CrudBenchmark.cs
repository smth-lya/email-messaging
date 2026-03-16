using System.Data;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Dapper;
using Homework.Benchmarks.Database;
using Homework.Benchmarks.Generators;
using Homework.Benchmarks.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Homework.Benchmarks;

[MemoryDiagnoser, ThreadingDiagnoser]
[CategoriesColumn, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[RPlotExporter]
[KeepBenchmarkFiles]
public class CrudBenchmark
{
    private const string MasterConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";
    
    private readonly Random _random = new(100);

    private DatabaseManager _databaseManager = null!;
    private CategoriesGenerator _categoryGenerator = null!;
    private ProductsGenerator _productsGenerator = null!;

    private Guid _testProductId;
    private Guid _testCategoryId;
    private string _connectionString = null!;

    [Params(100, 100_000)]
    public int ProductCount { get; set; }

    public int CategoryCount { get; set; } = 20;
    
    [GlobalSetup]
    public async Task Setup()
    {
        _databaseManager = new DatabaseManager(MasterConnectionString, _random);

        await _databaseManager.CreateDatabaseAsync();
        await _databaseManager.CreateSchemaAsync();
        
        _connectionString = _databaseManager.ConnectionString;

        _categoryGenerator = new CategoriesGenerator(_connectionString, _random);
        await _categoryGenerator.GenerateCategories(CategoryCount);
        
        _productsGenerator = new ProductsGenerator(_connectionString, _random, _categoryGenerator.GetCategoryIds().ToList());
        await _productsGenerator.GenerateProducts(ProductCount);

        _testProductId = _productsGenerator.GetRandomProductId();
        _testCategoryId = _categoryGenerator.GetRandomCategoryId();
    }
    
    [BenchmarkCategory("GET"), Benchmark(Baseline = true)]
    public async Task<Product?> EF_GetById()
    {
        await using var context = new ApplicationDbContext(_connectionString);
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.ProductId == _testProductId);
    }

    [BenchmarkCategory("GET"), Benchmark]
    public async Task<Product?> Dapper_GetById()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection
            .QueryFirstOrDefaultAsync<Product>(
                "SELECT * FROM Products WHERE ProductId=@Id", 
                new { Id = _testProductId });
    }

    [BenchmarkCategory("GET"), Benchmark]
    public async Task<Product?> AdoNet_GetById()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Products WHERE ProductId=@Id LIMIT 1";
        command.Parameters.Add(new NpgsqlParameter("Id", _testProductId));
        
        await using var reader = await command.ExecuteReaderAsync();
    
        if (!await reader.ReadAsync())
            return null;

        return new Product()
        {
            ProductId = reader.GetGuid("productid"),
            Name = reader.GetString("name"),
            Description = reader.GetString("description"),
            Price = reader.GetDecimal("price"),
            CategoryId = reader.GetGuid("categoryid"),
            Stock = reader.GetInt32("stock"),
            IsDeleted = reader.GetBoolean("isdeleted")
        };
    }


    [BenchmarkCategory("ADD"), Benchmark(Baseline = true)]
    public async Task<Guid> EF_Add()
    {
        var product = new Product()
        {
            ProductId = Guid.NewGuid(),
            Name = "TestProduct",
            Description = "TestDescription",
            Price = 100,
            CategoryId = _testCategoryId,
            Stock = 150,
            IsDeleted = false
        };
        
        await using var context = new ApplicationDbContext(_connectionString);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        
        return product.ProductId;
    }

    [BenchmarkCategory("ADD"), Benchmark]
    public async Task<Guid> Dapper_Add()
    {
        var product = new Product()
        {
            ProductId = Guid.NewGuid(),
            Name = "TestProduct",
            Description = "TestDescription",
            Price = 100,
            CategoryId = _testCategoryId,
            Stock = 150,
            IsDeleted = false
        };
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync("INSERT INTO Products VALUES (@ProductId, @Name, @Description, @Price, @CategoryId, @Stock, @IsDeleted)", product);
        
        return product.ProductId;
    }

    [BenchmarkCategory("ADD"), Benchmark]
    public async Task<Guid> AdoNet_Add()
    {
        var product = new Product()
        {
            ProductId = Guid.NewGuid(),
            Name = "TestProduct",
            Description = "TestDescription",
            Price = 100,
            CategoryId = _testCategoryId,
            Stock = 150,
            IsDeleted = false
        };
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Products VALUES (@ProductId, @Name, @Description, @Price, @CategoryId, @Stock, @IsDeleted)";
        
        var parameters = new[]
        {
            new NpgsqlParameter("productid", product.ProductId),
            new NpgsqlParameter("name", product.Name),
            new NpgsqlParameter("description", product.Description),
            new NpgsqlParameter("price", product.Price),
            new NpgsqlParameter("categoryId", product.CategoryId),
            new NpgsqlParameter("stock", product.Stock),
            new NpgsqlParameter("isdeleted", product.IsDeleted)
        };
        command.Parameters.AddRange(parameters);
        
        await command.ExecuteNonQueryAsync();
        
        return product.ProductId;
    }
    
    
    [BenchmarkCategory("UPDATE"), Benchmark(Baseline = true)]
    public async Task EF_Update()
    {
        var product = _productsGenerator.GenerateProduct();
        product.ProductId = _testProductId;
        
        await using var context = new ApplicationDbContext(_connectionString);
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }

    [BenchmarkCategory("UPDATE"), Benchmark]
    public async Task Dapper_Update()
    {
        var product = _productsGenerator.GenerateProduct();
        product.ProductId = _testProductId;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """
                UPDATE products 
                SET 
                    name = @Name, 
                    description = @Description, 
                    price = @Price,
                    categoryid = @CategoryId, 
                    stock = @Stock, 
                    isdeleted = @IsDeleted
                WHERE productid = @ProductId       
                """,
                product);
    }

    [BenchmarkCategory("UPDATE"), Benchmark]
    public async Task AdoNet_Update()
    {
        var product = _productsGenerator.GenerateProduct();
        product.ProductId = _testProductId;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = connection.CreateCommand();
        command.CommandText = """
                            UPDATE products 
                            SET 
                                name = @Name, 
                                description = @Description, 
                                price = @Price,
                                categoryid = @CategoryId, 
                                stock = @Stock, 
                                isdeleted = @IsDeleted
                            WHERE 
                                productid = @ProductId       
                            """;
                            
        var parameters = new[]
        {
            new NpgsqlParameter("productid", product.ProductId),
            new NpgsqlParameter("name", product.Name),
            new NpgsqlParameter("description", product.Description),
            new NpgsqlParameter("price", product.Price),
            new NpgsqlParameter("categoryId", product.CategoryId),
            new NpgsqlParameter("stock", product.Stock),
            new NpgsqlParameter("isdeleted", product.IsDeleted)
        };
        command.Parameters.AddRange(parameters);
        
        await command.ExecuteNonQueryAsync();
    }
    
    
    [BenchmarkCategory("DELETE"), Benchmark(Baseline = true)]
    public async Task EF_Delete()
    {
        await using var context = new ApplicationDbContext(_connectionString);
        
        var product = await context.Products.FindAsync(_testProductId);
        if (product is null)
            return;
        
        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }

    [BenchmarkCategory("DELETE"), Benchmark]
    public async Task Dapper_Delete()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM products WHERE productid = @ProductId", new { ProductId = _testProductId });
    }

    [BenchmarkCategory("DELETE"), Benchmark]
    public async Task AdoNet_Delete()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM products WHERE productid = @ProductId";
        command.Parameters.Add(new NpgsqlParameter("productid", _testProductId));
        
        await command.ExecuteNonQueryAsync();
    }
    
    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _databaseManager.DropDatabaseAsync();
    }
}