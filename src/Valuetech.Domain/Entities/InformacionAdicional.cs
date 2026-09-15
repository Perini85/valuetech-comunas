namespace Valuetech.Domain.Entities;

public sealed record InformacionAdicional
{
    public InformacionAdicional(decimal superficie, int poblacion, decimal densidad)
    {
        if (superficie <= 0) throw new ArgumentOutOfRangeException(nameof(superficie));
        if (poblacion < 0) throw new ArgumentOutOfRangeException(nameof(poblacion));
        if (densidad < 0) throw new ArgumentOutOfRangeException(nameof(densidad));
        Superficie = superficie;
        Poblacion = poblacion;
        Densidad = densidad;
    }

    public decimal Superficie { get; }
    public int Poblacion { get; }
    public decimal Densidad { get; }
}
