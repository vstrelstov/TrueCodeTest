using MediatR;

namespace TrueCodeTest.UserService.Application.Commands.Register;

public sealed record RegisterCommand(string Name, string Password) : IRequest<RegisterResult>;

public sealed record RegisterResult(int UserId, string Token);
