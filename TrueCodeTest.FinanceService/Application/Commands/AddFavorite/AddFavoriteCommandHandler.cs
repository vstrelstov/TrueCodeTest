using MediatR;
using TrueCodeTest.Shared.Entities;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Application.Commands.AddFavorite;

public sealed class AddFavoriteCommandHandler(
    ICurrencyRepository currencyRepository,
    IFavoriteRepository favoriteRepository)
    : IRequestHandler<AddFavoriteCommand>
{
    public async Task Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        if (await currencyRepository.FindByIdAsync(request.CurrencyId, cancellationToken) is null)
        {
            throw new KeyNotFoundException($"Валюта с id={request.CurrencyId} не найдена.");
        }

        if (await favoriteRepository.ExistsAsync(request.UserId, request.CurrencyId, cancellationToken))
        {
            // Идемпотентно: повторное добавление — не ошибка.
            return;
        }

        var favorite = new UserFavoriteCurrency
        {
            UserId = request.UserId,
            CurrencyId = request.CurrencyId,
        };

        await favoriteRepository.AddAsync(favorite, cancellationToken);
        await favoriteRepository.SaveChangesAsync(cancellationToken);
    }
}
