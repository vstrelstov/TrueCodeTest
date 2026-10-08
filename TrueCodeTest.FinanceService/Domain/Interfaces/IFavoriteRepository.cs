using TrueCodeTest.Shared.Entities;

namespace TrueCodeTest.FinanceService.Domain.Interfaces;

public interface IFavoriteRepository
{
    Task<List<Currency>> GetFavoritesAsync(int userId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int userId, int currencyId, CancellationToken cancellationToken);

    Task AddAsync(UserFavoriteCurrency favorite, CancellationToken cancellationToken);

    Task<bool> RemoveAsync(int userId, int currencyId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
