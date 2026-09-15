using Valuetech.Domain.Entities;

namespace Valuetech.Application.Abstractions.Persistence;

public interface IComunaRepository
{
    Task<IReadOnlyCollection<Comuna>> ListarPorRegionAsync(int regionId, CancellationToken cancellationToken);
    Task<Comuna?> ObtenerPorIdAsync(int regionId, int id, CancellationToken cancellationToken);
    Task<Comuna?> ActualizarAsync(Comuna comuna, CancellationToken cancellationToken);
}
