using MediatR;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Application.Commands.RemoveFavorite;

public sealed class RemoveFavoriteCommandHandler(IFavoriteRepository favoriteRepository)
    : IRequestHandler<RemoveFavoriteCommand>
{
    public async Task Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var removed = await favoriteRepository.RemoveAsync(
            request.UserId,
            request.CurrencyId,
            cancellationToken);

        if (!removed)
        {
            throw new KeyNotFoundException(
                $"Валюта с id={request.CurrencyId} не найдена в избранном пользователя {request.UserId}.");
        }

        await favoriteRepository.SaveChangesAsync(cancellationToken);
    }
}
