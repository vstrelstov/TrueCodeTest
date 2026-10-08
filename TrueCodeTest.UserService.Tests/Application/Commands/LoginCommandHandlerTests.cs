using TrueCodeTest.Shared.Entities;
using TrueCodeTest.UserService.Application.Commands.Login;
using TrueCodeTest.UserService.Application.Interfaces;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Tests.Application.Commands;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();

    private LoginCommandHandler CreateHandler()
        => new(_userRepository, _passwordHasher, _jwtTokenService);

    private static User MakeUser(string name = "alice", string hash = "hash")
        => new()
        {
            Id = 1,
            Name = name,
            Password = hash
        };

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var user = MakeUser();
        _userRepository.FindByNameAsync("alice", default).Returns(user);
        _passwordHasher.Verify("Secret1!", "hash").Returns(true);
        _jwtTokenService.GenerateToken(1, "alice").Returns("jwt-token");

        var result = await CreateHandler().Handle(new LoginCommand("alice", "Secret1!"), default);

        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorized()
    {
        _userRepository.FindByNameAsync("ghost", default).Returns((User?)null);

        var act = () => CreateHandler().Handle(new LoginCommand("ghost", "pass"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        var user = MakeUser();
        _userRepository.FindByNameAsync("alice", default).Returns(user);
        _passwordHasher.Verify("wrong", "hash").Returns(false);

        var act = () => CreateHandler().Handle(new LoginCommand("alice", "wrong"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_DoesNotGenerateToken()
    {
        var user = MakeUser();
        _userRepository.FindByNameAsync("alice", default).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        try
        {
            await CreateHandler().Handle(new LoginCommand("alice", "wrong"), default);
        }
        catch (UnauthorizedAccessException)
        {
        }

        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ErrorMessageDoesNotRevealReason()
    {
        _userRepository.FindByNameAsync("ghost", default).Returns((User?)null);

        var user = MakeUser();
        _userRepository.FindByNameAsync("alice", default).Returns(user);
        _passwordHasher.Verify("wrong", "hash").Returns(false);

        string? messageWhenNotFound = null;
        string? messageWhenWrongPassword = null;

        try
        {
            await CreateHandler().Handle(new LoginCommand("ghost", "pass"), default);
        }
        catch (UnauthorizedAccessException ex)
        {
            messageWhenNotFound = ex.Message;
        }

        try
        {
            await CreateHandler().Handle(new LoginCommand("alice", "wrong"), default);
        }
        catch (UnauthorizedAccessException ex)
        {
            messageWhenWrongPassword = ex.Message;
        }

        messageWhenNotFound.Should().Be(messageWhenWrongPassword);
    }
}
