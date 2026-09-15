using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Common.Exceptions;
using Valuetech.Domain.Entities;
using Valuetech.Infrastructure.Persistence.SqlServer;

namespace Valuetech.Infrastructure.Persistence.Repositories;

internal sealed class ComunaRepository(SqlConnectionFactory connectionFactory) : IComunaRepository
{
    public async Task<IReadOnlyCollection<Comuna>> ListarPorRegionAsync(int regionId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ComunaRow>(new CommandDefinition(
            "dbo.Comunas_ListarPorRegion", new { IdRegion = regionId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return rows.Select(x => x.ToEntity()).ToArray();
    }

    public async Task<Comuna?> ObtenerPorIdAsync(int regionId, int id, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ComunaRow>(new CommandDefinition(
            "dbo.Comunas_ObtenerPorId", new { IdRegion = regionId, IdComuna = id },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return row?.ToEntity();
    }

    public async Task<Comuna?> ActualizarAsync(Comuna comuna, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        try
        {
            var row = await connection.QuerySingleOrDefaultAsync<ComunaRow>(new CommandDefinition(
                "dbo.Comunas_Actualizar", new
                {
                    IdRegion = comuna.RegionId,
                    IdComuna = comuna.Id,
                    comuna.Nombre,
                    InformacionAdicional = InformacionAdicionalXml.Serialize(comuna.InformacionAdicional)
                }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return row?.ToEntity();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new ConflictException("Ya existe una comuna con ese nombre en la región.", ex);
        }
    }

    private sealed class ComunaRow
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? InformacionAdicional { get; set; }

        public Comuna ToEntity() => Comuna.Rehidratar(Id, RegionId, Nombre,
            InformacionAdicionalXml.Deserialize(InformacionAdicional));
    }
}
