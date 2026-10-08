using TrueCodeTest.FinanceService.Application.Commands.RemoveFavorite;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Tests.Application.Commands;

public sealed class RemoveFavoriteCommandHandlerTests
{
    private readonly IFavoriteRepository _favoriteRepository = Substitute.For<IFavoriteRepository>();

    private RemoveFavoriteCommandHandler CreateHandler()
        => new(_favoriteRepository);

    [Fact]
    public async Task Handle_ExistingFavorite_RemovesAndSaves()
    {
        _favoriteRepository.RemoveAsync(10, 1, default).Returns(true);

        await CreateHandler().Handle(new RemoveFavoriteCommand(10, 1), default);

        await _favoriteRepository.Received(1).RemoveAsync(10, 1, default);
        await _favoriteRepository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_NotInFavorites_ThrowsKeyNotFound()
    {
        _favoriteRepository.RemoveAsync(10, 999, default).Returns(false);

        var act = () => CreateHandler().Handle(new RemoveFavoriteCommand(10, 999), default);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_NotInFavorites_DoesNotSave()
    {
        _favoriteRepository.RemoveAsync(10, 999, default).Returns(false);

        try
        {
            await CreateHandler().Handle(new RemoveFavoriteCommand(10, 999), default);
        }
        catch (KeyNotFoundException)
        {
        }

        await _favoriteRepository.DidNotReceive().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ErrorMessageContainsUserAndCurrencyId()
    {
        _favoriteRepository.RemoveAsync(42, 7, default).Returns(false);

        var act = () => CreateHandler().Handle(new RemoveFavoriteCommand(42, 7), default);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*7*")
            .WithMessage("*42*");
    }
}
