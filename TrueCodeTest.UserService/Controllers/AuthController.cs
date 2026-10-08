using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueCodeTest.UserService.Application.Commands.Login;
using TrueCodeTest.UserService.Application.Commands.Logout;
using TrueCodeTest.UserService.Application.Commands.Register;

namespace TrueCodeTest.UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterCommand(request.Name, request.Password), cancellationToken);
        return CreatedAtAction(nameof(Register), new RegisterResponse(result.UserId, result.Token));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Name, request.Password), cancellationToken);
        return Ok(new TokenResponse(result.Token));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutCommand(), cancellationToken);
        return NoContent();
    }
}

public sealed record RegisterRequest(string Name, string Password);

public sealed record LoginRequest(string Name, string Password);

public sealed record RegisterResponse(int UserId, string Token);

public sealed record TokenResponse(string Token);
