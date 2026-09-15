using Microsoft.AspNetCore.Mvc;
using Valuetech.WebMvc.Services;

namespace Valuetech.WebMvc.Controllers;

public sealed class RegionesController(ValuetechApiClient api) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await api.ListarRegiones(cancellationToken));
}
