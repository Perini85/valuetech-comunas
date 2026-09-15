using MediatR;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Features.Regiones.Common;

namespace Valuetech.Application.Features.Regiones.ObtenerRegionPorId;

public sealed class ObtenerRegionPorIdQueryHandler(
    IRegionRepository regionRepository)
    : IRequestHandler<
        ObtenerRegionPorIdQuery,
        RegionDto?>
{
    public async Task<RegionDto?> Handle(
        ObtenerRegionPorIdQuery request,
        CancellationToken cancellationToken)
    {
        var region = await regionRepository.ObtenerPorIdAsync(
            request.Id,
            cancellationToken);

        return region is null
            ? null
            : new RegionDto(
                region.Id,
                region.Nombre);
    }
}