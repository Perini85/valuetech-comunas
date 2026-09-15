using System.Data;
using Dapper;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Domain.Entities;
using Valuetech.Infrastructure.Persistence.SqlServer;

namespace Valuetech.Infrastructure.Persistence.Repositories;

internal sealed class RegionRepository(
    SqlConnectionFactory connectionFactory)
    : IRegionRepository
{
    private const string ListarStoredProcedure =
        "dbo.Regiones_Listar";

    public async Task<IReadOnlyCollection<Region>> ListarAsync(
        CancellationToken cancellationToken)
    {
        await using var connection =
            connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: ListarStoredProcedure,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<RegionRow>(
            command);

        return rows
            .Select(row => Region.Rehidratar(
                row.Id,
                row.Nombre))
            .ToArray();
    }

    private sealed class RegionRow
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public async Task<Region?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await using var connection =
            connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: "dbo.Regiones_ObtenerPorId",
            parameters: new
            {
                IdRegion = id
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<RegionRow>(
            command);

        return row is null
            ? null
            : Region.Rehidratar(row.Id, row.Nombre);
    }
}
