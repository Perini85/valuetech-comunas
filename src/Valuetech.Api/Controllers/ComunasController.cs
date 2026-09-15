using MediatR;
using Microsoft.AspNetCore.Mvc;
using Valuetech.Api.Contracts;
using Valuetech.Application.Features.Comunas.ActualizarComuna;
using Valuetech.Application.Features.Comunas.Common;
using Valuetech.Application.Features.Comunas.ListarComunasPorRegion;
using Valuetech.Application.Features.Comunas.ObtenerComunaPorId;

namespace Valuetech.Api.Controllers;

[ApiController]
[Route("region/{regionId:int}/comuna")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public sealed class ComunasController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ComunaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ComunaDto>>> Listar(int regionId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new ListarComunasPorRegionQuery(regionId), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ComunaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComunaDto>> Obtener(int regionId, int id, CancellationToken cancellationToken)
        => Ok(await sender.Send(new ObtenerComunaPorIdQuery(regionId, id), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(ComunaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ComunaDto>> Actualizar(int regionId, ActualizarComunaRequest request,
        CancellationToken cancellationToken)
        => Ok(await sender.Send(new ActualizarComunaCommand(regionId, request.Id,
            request.Nombre, request.InformacionAdicional is { } info
                ? new InformacionAdicionalDto(info.Superficie, info.Poblacion, info.Densidad)
                : null), cancellationToken));
}
