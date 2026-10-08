using System.Net.Mime;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueCodeTest.FinanceService.Application.Commands.AddFavorite;
using TrueCodeTest.FinanceService.Application.Commands.RemoveFavorite;
using TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;

namespace TrueCodeTest.FinanceService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public sealed class CurrenciesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Курсы валют из избранного пользователя.
    /// Если избранных нет — возвращаются все доступные валюты.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrencies(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetUserCurrenciesQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("favorites/{currencyId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFavorite(int currencyId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await mediator.Send(new AddFavoriteCommand(userId, currencyId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("favorites/{currencyId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(int currencyId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await mediator.Send(new RemoveFavoriteCommand(userId, currencyId), cancellationToken);
        return NoContent();
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Клейм sub отсутствует в токене.");

        return int.Parse(sub);
    }
}
