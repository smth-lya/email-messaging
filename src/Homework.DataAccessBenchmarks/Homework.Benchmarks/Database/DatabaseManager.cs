using Npgsql;

namespace Homework.Benchmarks;

public class DatabaseManager : IAsyncDisposable
{
    private readonly string _masterConnectionString;
    private readonly string _databaseName;
    private readonly string _testConnectionString;
    private readonly Random _random;
    
    public string ConnectionString => _testConnectionString;
    
    public DatabaseManager(string masterConnectionString, Random random)
    {
        _masterConnectionString = masterConnectionString;
        _random = random;
        
        _databaseName = $"benchmark_db_{DateTime.Now:yyyyMMdd_HHmmss}_{_random.Next(1000, 9999)}";
        
        var builder = new NpgsqlConnectionStringBuilder(masterConnectionString)
        {
            Database = _databaseName
        };
        _testConnectionString = builder.ToString();
    }
    
    public async Task CreateDatabaseAsync()
    {
        await using var masterConnection = new NpgsqlConnection(_masterConnectionString);
        await masterConnection.OpenAsync();
        
        var exists = await CheckDatabaseExistsAsync(masterConnection);
        if (exists)
        {
            await DropDatabaseAsync();
        }
        
        await using var createCommand = new NpgsqlCommand(
            $"CREATE DATABASE \"{_databaseName}\" ENCODING = 'UTF8'", 
            masterConnection);
        await createCommand.ExecuteNonQueryAsync();
        
        Console.WriteLine($"[DatabaseManager] Created database: {_databaseName}");
    }
    
    private async Task<bool> CheckDatabaseExistsAsync(NpgsqlConnection connection)
    {
        await using var checkCommand = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @dbName",
            connection);
        checkCommand.Parameters.AddWithValue("dbName", _databaseName);
        
        var result = await checkCommand.ExecuteScalarAsync();
        return result != null;
    }
    
    public async Task CreateSchemaAsync()
    {
        await using var testConnection = new NpgsqlConnection(_testConnectionString);
        await testConnection.OpenAsync();
        
        await using var createCategories = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS Categories (
                CategoryId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                Name VARCHAR(50) NOT NULL,
                Description VARCHAR(300) NULL
            )", testConnection);
        await createCategories.ExecuteNonQueryAsync();
        
        await using var createProducts = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS Products (
                ProductId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                Name VARCHAR(50) NOT NULL,
                Description VARCHAR(300) NOT NULL,
                Price DECIMAL(18,2) NOT NULL,
                CategoryId UUID NOT NULL,
                Stock INTEGER NOT NULL,
                IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
                CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) 
                    REFERENCES Categories(CategoryId) ON DELETE CASCADE
            )", testConnection);
        await createProducts.ExecuteNonQueryAsync();
        
        await using var index1 = new NpgsqlCommand(
            "CREATE INDEX IX_Products_CategoryId ON Products(CategoryId)", 
            testConnection);
        await index1.ExecuteNonQueryAsync();
        
        await using var index2 = new NpgsqlCommand(
            "CREATE INDEX IX_Products_IsDeleted ON Products(IsDeleted) WHERE IsDeleted = FALSE", 
            testConnection);
        await index2.ExecuteNonQueryAsync();
        
        Console.WriteLine($"[DatabaseManager] Created schema in: {_databaseName}");
    }
    
    public async Task DropDatabaseAsync()
    {
        try
        {
            await using var masterConnection = new NpgsqlConnection(_masterConnectionString);
            await masterConnection.OpenAsync();
            
            await using var terminateCommand = new NpgsqlCommand(@"
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = @dbName AND pid <> pg_backend_pid()",
                masterConnection);
            terminateCommand.Parameters.AddWithValue("dbName", _databaseName);
            await terminateCommand.ExecuteNonQueryAsync();
            
            await using var dropCommand = new NpgsqlCommand(
                $"DROP DATABASE IF EXISTS \"{_databaseName}\"", 
                masterConnection);
            await dropCommand.ExecuteNonQueryAsync();
            
            Console.WriteLine($"[DatabaseManager] Dropped database: {_databaseName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseManager] Error dropping database: {ex.Message}");
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        await DropDatabaseAsync();
        GC.SuppressFinalize(this);
    }
}