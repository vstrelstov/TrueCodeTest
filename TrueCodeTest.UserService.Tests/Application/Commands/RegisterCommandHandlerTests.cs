using TrueCodeTest.Shared.Entities;
using TrueCodeTest.UserService.Application.Commands.Register;
using TrueCodeTest.UserService.Application.Interfaces;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Tests.Application.Commands;

public sealed class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();

    private RegisterCommandHandler CreateHandler()
        => new(_userRepository, _passwordHasher, _jwtTokenService);

    [Fact]
    public async Task Handle_NewUser_ReturnsTokenAndUserId()
    {
        _userRepository.ExistsByNameAsync("alice", default)
            .Returns(false);
        _passwordHasher.Hash("Secret1!")
            .Returns("hashed");
        _jwtTokenService.GenerateToken(Arg.Any<int>(), "alice")
            .Returns("jwt-token");
        var command = new RegisterCommand("alice", "Secret1!");

        var result = await CreateHandler().Handle(command, default);

        result.Token.Should().Be("jwt-token");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Name == "alice" && u.Password == "hashed"),
            default);
        await _userRepository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsInvalidOperationException()
    {
        _userRepository.ExistsByNameAsync("alice", default)
            .Returns(true);
        var command = new RegisterCommand("alice", "Secret1!");

        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*alice*");
    }

    [Fact]
    public async Task Handle_DuplicateName_DoesNotSave()
    {
        _userRepository.ExistsByNameAsync("alice", default)
            .Returns(true);
        var command = new RegisterCommand("alice", "Secret1!");

        try
        {
            await CreateHandler().Handle(command, default);
        }
        catch (InvalidOperationException) { }

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), default);
        await _userRepository.DidNotReceive().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_NewUser_PasswordIsHashed()
    {
        _userRepository.ExistsByNameAsync("bob", default).Returns(false);
        _passwordHasher.Hash("plain").Returns("$2b$hash");
        _jwtTokenService.GenerateToken(Arg.Any<int>(), Arg.Any<string>()).Returns("t");

        await CreateHandler().Handle(new RegisterCommand("bob", "plain"), default);

        _passwordHasher.Received(1).Hash("plain");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Password == "$2b$hash"),
            default);
    }
}
