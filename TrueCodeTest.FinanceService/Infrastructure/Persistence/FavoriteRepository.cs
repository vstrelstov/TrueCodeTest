using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Data;
using TrueCodeTest.Shared.Entities;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Infrastructure.Persistence;

public sealed class FavoriteRepository(AppDbContext dbContext) : IFavoriteRepository
{
    public Task<List<Currency>> GetFavoritesAsync(int userId, CancellationToken cancellationToken)
        => dbContext.UserFavoriteCurrencies
            .Where(f => f.UserId == userId)
            .Select(f => f.Currency)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int userId, int currencyId, CancellationToken cancellationToken)
        => dbContext.UserFavoriteCurrencies
            .AnyAsync(f => f.UserId == userId && f.CurrencyId == currencyId, cancellationToken);

    public async Task AddAsync(UserFavoriteCurrency favorite, CancellationToken cancellationToken)
        => await dbContext.UserFavoriteCurrencies.AddAsync(favorite, cancellationToken);

    public async Task<bool> RemoveAsync(int userId, int currencyId, CancellationToken cancellationToken)
    {
        var row = await dbContext.UserFavoriteCurrencies
            .FirstOrDefaultAsync(f => f.UserId == userId && f.CurrencyId == currencyId, cancellationToken);

        if (row is null) return false;

        dbContext.UserFavoriteCurrencies.Remove(row);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
