using MediatR;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Features.Regiones.Common;

namespace Valuetech.Application.Features.Regiones.ListarRegiones;

public sealed class ListarRegionesQueryHandler(
    IRegionRepository regionRepository)
    : IRequestHandler<
        ListarRegionesQuery,
        IReadOnlyCollection<RegionDto>>
{
    public async Task<IReadOnlyCollection<RegionDto>> Handle(
        ListarRegionesQuery request,
        CancellationToken cancellationToken)
    {
        var regiones = await regionRepository.ListarAsync(
            cancellationToken);

        return regiones
            .Select(region => new RegionDto(
                region.Id,
                region.Nombre))
            .ToArray();
    }
}