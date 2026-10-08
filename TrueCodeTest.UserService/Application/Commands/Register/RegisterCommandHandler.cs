using MediatR;
using TrueCodeTest.Shared.Entities;
using TrueCodeTest.UserService.Application.Interfaces;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Application.Commands.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Пользователь с именем '{request.Name}' уже существует.");
        }

        var user = new User
        {
            Name = request.Name,
            Password = passwordHasher.Hash(request.Password),
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.GenerateToken(user.Id, user.Name);

        return new RegisterResult(user.Id, token);
    }
}
