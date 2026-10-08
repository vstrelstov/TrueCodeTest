using MediatR;

namespace TrueCodeTest.FinanceService.Application.Commands.RemoveFavorite;

public sealed record RemoveFavoriteCommand(int UserId, int CurrencyId) : IRequest;
