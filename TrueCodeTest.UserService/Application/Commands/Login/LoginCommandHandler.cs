using MediatR;
using TrueCodeTest.UserService.Application.Interfaces;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByNameAsync(request.Name, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.Password))
        {
            // Одинаковое сообщение для обоих случаев, чтобы не раскрывать,
            // зарегистрирован ли пользователь.
            throw new UnauthorizedAccessException("Неверное имя пользователя или пароль.");
        }

        var token = jwtTokenService.GenerateToken(user.Id, user.Name);

        return new LoginResult(token);
    }
}
