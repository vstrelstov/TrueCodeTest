using MediatR;
using TrueCodeTest.FinanceService.Application.DTOs;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;

public sealed class GetUserCurrenciesQueryHandler(
    ICurrencyRepository currencyRepository,
    IFavoriteRepository favoriteRepository)
    : IRequestHandler<GetUserCurrenciesQuery, List<CurrencyDto>>
{
    public async Task<List<CurrencyDto>> Handle(
        GetUserCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var favorites = await favoriteRepository.GetFavoritesAsync(request.UserId, cancellationToken);

        var currencies = favorites.Count > 0
            ? favorites
            : await currencyRepository.GetAllAsync(cancellationToken);

        return currencies
            .Select(c => new CurrencyDto(c.Id, c.Name, c.Rate))
            .ToList();
    }
}
