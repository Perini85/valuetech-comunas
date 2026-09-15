using Valuetech.Domain.Entities;

namespace Valuetech.Application.Features.Comunas.Common;

public sealed record InformacionAdicionalDto(decimal Superficie, int Poblacion, decimal Densidad);

public sealed record ComunaDto(int Id, int RegionId, string Nombre, InformacionAdicionalDto? InformacionAdicional)
{
    public static ComunaDto FromEntity(Comuna comuna) => new(
        comuna.Id, comuna.RegionId, comuna.Nombre,
        comuna.InformacionAdicional is { } info
            ? new InformacionAdicionalDto(info.Superficie, info.Poblacion, info.Densidad)
            : null);
}
