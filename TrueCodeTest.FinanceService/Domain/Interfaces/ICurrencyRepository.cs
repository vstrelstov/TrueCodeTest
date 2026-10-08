using TrueCodeTest.Shared.Entities;

namespace TrueCodeTest.FinanceService.Domain.Interfaces;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetAllAsync(CancellationToken cancellationToken);

    Task<Currency?> FindByIdAsync(int id, CancellationToken cancellationToken);
}
