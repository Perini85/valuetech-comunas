namespace Valuetech.Domain.Entities;

public sealed class Region
{
    private Region(int id, string nombre)
    {
        Id = id;
        Nombre = ValidarNombre(nombre);
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public static Region Crear(string nombre)
        => new(0, nombre);

    public static Region Rehidratar(int id, string nombre)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El identificador de la región debe ser mayor que cero.");
        }

        return new Region(id, nombre);
    }

    public void Renombrar(string nombre)
        => Nombre = ValidarNombre(nombre);

    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de la región es obligatorio.",
                nameof(nombre));
        }

        return nombre.Trim();
    }
}