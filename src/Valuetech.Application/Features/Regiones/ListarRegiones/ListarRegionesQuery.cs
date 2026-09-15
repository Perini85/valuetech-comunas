using MediatR;
using Valuetech.Application.Features.Regiones.Common;

namespace Valuetech.Application.Features.Regiones.ListarRegiones;

public sealed record ListarRegionesQuery()
    : IRequest<IReadOnlyCollection<RegionDto>>;