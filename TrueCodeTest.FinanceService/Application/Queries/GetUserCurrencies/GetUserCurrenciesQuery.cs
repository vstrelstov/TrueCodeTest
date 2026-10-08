using MediatR;
using TrueCodeTest.FinanceService.Application.DTOs;

namespace TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;

public sealed record GetUserCurrenciesQuery(int UserId) : IRequest<List<CurrencyDto>>;
