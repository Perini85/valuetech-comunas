using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Valuetech.Api.Contracts;

public sealed record ActualizarComunaRequest(int Id, [Required] string Nombre,
    InformacionAdicionalRequest? InformacionAdicional);

public sealed record InformacionAdicionalRequest(
    [property: JsonRequired] decimal Superficie,
    [property: JsonRequired] int Poblacion,
    [property: JsonRequired] decimal Densidad);
