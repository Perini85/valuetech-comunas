using System.Net;
using Microsoft.AspNetCore.Mvc;
using Valuetech.WebMvc.Models;
using Valuetech.WebMvc.Services;

namespace Valuetech.WebMvc.Controllers;

[Route("regiones/{regionId:int}/comunas")]
public sealed class ComunasController(ValuetechApiClient api) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int regionId, CancellationToken cancellationToken)
    {
        var region = await api.ObtenerRegion(regionId, cancellationToken);
        var comunas = await api.ListarComunas(regionId, cancellationToken);
        return View(new ComunasIndexModel(region, comunas));
    }

    [HttpGet("{id:int}/editar")]
    public async Task<IActionResult> Editar(int regionId, int id, CancellationToken cancellationToken)
    {
        var comuna = await api.ObtenerComuna(regionId, id, cancellationToken);
        var region = await api.ObtenerRegion(regionId, cancellationToken);
        return View(new EditarComunaModel
        {
            Id = comuna.Id, RegionId = comuna.RegionId, RegionNombre = region.Nombre,
            Nombre = comuna.Nombre, Superficie = comuna.InformacionAdicional?.Superficie,
            Poblacion = comuna.InformacionAdicional?.Poblacion, Densidad = comuna.InformacionAdicional?.Densidad
        });
    }

    [HttpPost("{id:int}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int regionId, int id,
        [Bind("Id,RegionId,Nombre,Superficie,Poblacion,Densidad")] EditarComunaModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id || regionId != model.RegionId) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await api.ActualizarComuna(model, cancellationToken);
                TempData["Success"] = "Los cambios de la comuna se guardaron correctamente.";
                return RedirectToAction(nameof(Index), new { regionId });
            }
            catch (ApiException ex) when (ex.StatusCode != HttpStatusCode.NotFound)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                foreach (var error in ex.Errors)
                    foreach (var message in error.Value)
                        ModelState.AddModelError(error.Key.Replace("InformacionAdicional.", string.Empty), message);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio. Tus datos siguen en el formulario; intenta guardar nuevamente.");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                ModelState.AddModelError(string.Empty, "El servicio tardó demasiado. Revisa el listado antes de volver a guardar.");
            }
        }
        return View(model);
    }
}
