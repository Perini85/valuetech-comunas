namespace Valuetech.Domain.Entities;

public sealed class Comuna
{
    private Comuna(int id, int regionId, string nombre)
    {
        Id = id;
        RegionId = ValidarRegionId(regionId);
        Nombre = ValidarNombre(nombre);
    }

    public int Id { get; private set; }

    public int RegionId { get; private set; }

    public string Nombre { get; private set; }

    public static Comuna Crear(int regionId, string nombre)
        => new(0, regionId, nombre);

    public static Comuna Rehidratar(
        int id,
        int regionId,
        string nombre)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El identificador de la comuna debe ser mayor que cero.");
        }

        return new Comuna(id, regionId, nombre);
    }

    public void Renombrar(string nombre)
        => Nombre = ValidarNombre(nombre);

    public void CambiarRegion(int regionId)
        => RegionId = ValidarRegionId(regionId);

    private static int ValidarRegionId(int regionId)
    {
        if (regionId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(regionId),
                "El identificador de la región debe ser mayor que cero.");
        }

        return regionId;
    }

    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de la comuna es obligatorio.",
                nameof(nombre));
        }

        return nombre.Trim();
    }
}