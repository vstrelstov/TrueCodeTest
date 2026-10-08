using MediatR;

namespace TrueCodeTest.FinanceService.Application.Commands.AddFavorite;

public sealed record AddFavoriteCommand(int UserId, int CurrencyId) : IRequest;
