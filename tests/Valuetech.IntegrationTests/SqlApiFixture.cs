using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Valuetech.Infrastructure;

namespace Valuetech.IntegrationTests;

public sealed class SqlFactAttribute : FactAttribute
{
    public SqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VALUETECH_TEST_SQLSERVER")))
            Skip = "Configura VALUETECH_TEST_SQLSERVER para ejecutar contra una base temporal de SQL Server.";
    }
}

public sealed class SqlApiFixture : IAsyncLifetime
{
    private readonly string databaseName = "ValuetechTests_" + Guid.NewGuid().ToString("N");
    private string? adminConnection;
    public string ConnectionString { get; private set; } = string.Empty;
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var setting = Environment.GetEnvironmentVariable("VALUETECH_TEST_SQLSERVER");
        if (string.IsNullOrWhiteSpace(setting)) return;
        var connection = new SqlConnectionStringBuilder(setting) { InitialCatalog = "master" };
        adminConnection = connection.ConnectionString;
        connection.InitialCatalog = databaseName;
        ConnectionString = connection.ConnectionString;
        try
        {
            await using var sql = new SqlConnection(adminConnection);
            await sql.OpenAsync();
            foreach (var filename in new[] { "001_schema.sql", "002_stored_procedures.sql", "003_seed.sql" })
            {
                var script = (await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "database", filename)))
                    .Replace("ValuetechComunas", databaseName, StringComparison.Ordinal);
                foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    await using var command = new SqlCommand(batch, sql);
                    await command.ExecuteNonQueryAsync();
                }
            }

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                // Sustituir la composición de persistencia: la aplicación captura la cadena
                // al registrar Infrastructure, antes de ConfigureAppConfiguration del test host.
                builder.ConfigureServices(services => services.AddInfrastructure(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    { ["ConnectionStrings:SqlServer"] = ConnectionString }).Build()));
            });
            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory is not null) await Factory.DisposeAsync();
        if (adminConnection is null) return;
        SqlConnection.ClearAllPools();
        await using var sql = new SqlConnection(adminConnection);
        await sql.OpenAsync();
        // El nombre proviene exclusivamente del GUID generado por esta fixture.
        await using var command = new SqlCommand($"IF DB_ID(N'{databaseName}') IS NOT NULL BEGIN ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}]; END", sql);
        await command.ExecuteNonQueryAsync();
    }
}
