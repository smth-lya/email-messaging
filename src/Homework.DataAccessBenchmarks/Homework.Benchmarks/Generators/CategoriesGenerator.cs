using System.Collections.ObjectModel;
using Bogus;
using Dapper;
using Homework.Benchmarks.Models;
using Npgsql;

namespace Homework.Benchmarks;

public class CategoriesGenerator
{
    private readonly string _connectionString;
    private readonly List<Guid> _categoryIds = new();
    private readonly Faker<Category> _categoryGenerator;

    private readonly Random _random;
    
    public CategoriesGenerator(string connectionString, Random random)
    {
        _random = random;
        _connectionString = connectionString;
        Randomizer.Seed = random;
        
        _categoryGenerator = new Faker<Category>()
            .RuleFor(c => c.CategoryId, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.Description, f => f.Lorem.Sentence());
    }

    public async Task GenerateCategories(int count)
    {
        var categories = _categoryGenerator.Generate(count);
        
        await using var connection = new NpgsqlConnection(_connectionString);
        const string sql = "INSERT INTO Categories (CategoryId, Name, Description) VALUES (@CategoryId, @Name, @Description)";
        await connection.ExecuteAsync(sql, categories);
        
        _categoryIds.AddRange(categories.Select(c => c.CategoryId));
    }

    public IReadOnlyList<Guid> GetCategoryIds() => _categoryIds.AsReadOnly();
    
    public Guid GetRandomCategoryId()
    {
        if (_categoryIds.Count == 0)
            throw new InvalidOperationException("No products have been generated yet");
            
        return _categoryIds[_random.Next(_categoryIds.Count)];
    }
    
    public async Task CleanupCategories()
    {
        if (_categoryIds.Count == 0) return;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM Categories WHERE CategoryId = ANY(@Ids)", new { Ids = _categoryIds });
        _categoryIds.Clear();
    }
}