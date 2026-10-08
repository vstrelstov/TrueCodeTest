using MediatR;

namespace TrueCodeTest.UserService.Application.Commands.Login;

public sealed record LoginCommand(string Name, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(string Token);
