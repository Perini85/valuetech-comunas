using System.ComponentModel.DataAnnotations;

namespace Valuetech.WebMvc.Models;

public sealed record RegionModel(int Id, string Nombre);
public sealed record InformacionAdicionalModel(decimal Superficie, int Poblacion, decimal Densidad);
public sealed record ComunaModel(int Id, int RegionId, string Nombre, InformacionAdicionalModel? InformacionAdicional);
public sealed record ComunasIndexModel(RegionModel Region, IReadOnlyList<ComunaModel> Comunas);

public sealed class EditarComunaModel : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
    [Range(1, int.MaxValue)]
    public int RegionId { get; set; }
    public string? RegionNombre { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre de la comuna")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Superficie (km²)")]
    public decimal? Superficie { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La población no puede ser negativa.")]
    [Display(Name = "Población (habitantes)")]
    public int? Poblacion { get; set; }

    [Display(Name = "Densidad (hab./km²)")]
    public decimal? Densidad { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Superficie.HasValue || Poblacion.HasValue || Densidad.HasValue)
        {
            if (!Superficie.HasValue || Superficie <= 0)
                yield return new ValidationResult("Ingresa una superficie mayor que cero.", [nameof(Superficie)]);
            if (!Poblacion.HasValue)
                yield return new ValidationResult("Ingresa la población.", [nameof(Poblacion)]);
            if (!Densidad.HasValue || Densidad < 0)
                yield return new ValidationResult("Ingresa una densidad mayor o igual que cero.", [nameof(Densidad)]);
        }
    }
}
