using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Data;
using TrueCodeTest.Shared.Entities;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Infrastructure.Persistence;

public sealed class CurrencyRepository(AppDbContext dbContext) : ICurrencyRepository
{
    public Task<List<Currency>> GetAllAsync(CancellationToken cancellationToken)
        => dbContext.Currencies.ToListAsync(cancellationToken);

    public Task<Currency?> FindByIdAsync(int id, CancellationToken cancellationToken)
        => dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
