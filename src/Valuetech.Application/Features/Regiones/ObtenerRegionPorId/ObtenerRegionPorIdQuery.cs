using MediatR;
using Valuetech.Application.Features.Regiones.Common;

namespace Valuetech.Application.Features.Regiones.ObtenerRegionPorId;

public sealed record ObtenerRegionPorIdQuery(int Id)
    : IRequest<RegionDto?>;