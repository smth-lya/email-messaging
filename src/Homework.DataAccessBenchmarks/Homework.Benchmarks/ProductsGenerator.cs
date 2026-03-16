using System.Data;
using Bogus;
using Dapper;
using Homework.Benchmarks.Models;
using Npgsql;

namespace Homework.Benchmarks;

public class ProductsGenerator
{
    private readonly string _connectionString;
    private readonly List<Guid> _productIds = new();
    private readonly List<Guid> _categoryIds;
    private readonly Faker<Product> _productGenerator;

    private readonly Random _random;
    
    public ProductsGenerator(string connectionString, Random random, List<Guid> existingCategoryIds)
    {
        Randomizer.Seed = random;
        _random = random;
        _connectionString = connectionString;
        _categoryIds = existingCategoryIds;
        
        _productGenerator = new Faker<Product>()
            .RuleFor(p => p.ProductId, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => f.Random.Decimal(0.0m, 1000.0m))
            .RuleFor(p => p.CategoryId, f => f.PickRandom(_categoryIds))
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 500))
            .RuleFor(p => p.IsDeleted, f => f.Random.Bool(0.1f));
    }

    public async Task GenerateProducts(int count)
    {
        var generatedProducts = _productGenerator.Generate(count);

        await using var connection = new NpgsqlConnection(_connectionString);
        const string query = """
                             INSERT INTO Products (ProductId, Name, Description, Price, CategoryId, Stock, IsDeleted)
                             VALUES (@ProductId, @Name, @Description, @Price, @CategoryId, @Stock, @IsDeleted)
                             """;
        
        await connection.ExecuteAsync(query, generatedProducts);
        _productIds.AddRange(generatedProducts.Select(p => p.ProductId));
    }

    public Guid GetRandomProductId()
    {
        if (_productIds.Count == 0)
            throw new InvalidOperationException("No products have been generated yet");
            
        return _productIds[_random.Next(_productIds.Count)];
    }

    
    public async Task CleanupProducts()
    {
        if (_productIds.Count == 0) 
            return;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM Products WHERE ProductId = ANY(@Ids)", new { Ids = _productIds });
        
        _productIds.Clear();
    }
}