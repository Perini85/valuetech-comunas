using MediatR;
using Valuetech.Application.Features.Comunas.Common;

namespace Valuetech.Application.Features.Comunas.ActualizarComuna;

public sealed record ActualizarComunaCommand(int RegionId, int Id, string Nombre,
    InformacionAdicionalDto? InformacionAdicional) : IRequest<ComunaDto>;
