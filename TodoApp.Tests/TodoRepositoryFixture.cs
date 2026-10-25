using Microsoft.Extensions.Configuration;
using MySqlConnector;
using TodoApp.Data;

namespace TodoApp.Tests;

public class TodoRepositoryFixture
{
    public TodoRepository Repository { get; }
    private readonly string _connectionString;

    public TodoRepositoryFixture()
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.Test.json", optional: false)
        .Build();

    _connectionString = configuration.GetConnectionString("TodoDb")
        ?? throw new InvalidOperationException(
            "Test connection string not configured.");

    var connectionSettings =
        new MySqlConnectionStringBuilder(_connectionString);

    if (connectionSettings.Database != "todo_test_db")
    {
        throw new InvalidOperationException(
            $"Tests geblokkeerd: database is '{connectionSettings.Database}', " +
            "maar moet 'todo_test_db' zijn.");
    }

    Repository = new TodoRepository(configuration);
}

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "TRUNCATE TABLE todos;";
        await command.ExecuteNonQueryAsync();
    }
}
