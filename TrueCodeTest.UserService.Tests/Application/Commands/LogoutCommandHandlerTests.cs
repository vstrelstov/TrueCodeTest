using TrueCodeTest.UserService.Application.Commands.Logout;

namespace TrueCodeTest.UserService.Tests.Application.Commands;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_Always_CompletesSuccessfully()
    {
        var handler = new LogoutCommandHandler();

        var act = () => handler.Handle(new LogoutCommand(), default);

        await act.Should().NotThrowAsync();
    }
}
