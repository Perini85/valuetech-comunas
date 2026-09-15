using Valuetech.Domain.Entities;

namespace Valuetech.Application.Abstractions.Persistence;

public interface IRegionRepository
{
    Task<IReadOnlyCollection<Region>> ListarAsync(
        CancellationToken cancellationToken);
}