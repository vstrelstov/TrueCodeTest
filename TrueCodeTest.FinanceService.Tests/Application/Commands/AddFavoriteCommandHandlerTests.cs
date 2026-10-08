using TrueCodeTest.FinanceService.Application.Commands.AddFavorite;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Entities;

namespace TrueCodeTest.FinanceService.Tests.Application.Commands;

public sealed class AddFavoriteCommandHandlerTests
{
    private readonly ICurrencyRepository _currencyRepository = Substitute.For<ICurrencyRepository>();
    private readonly IFavoriteRepository _favoriteRepository = Substitute.For<IFavoriteRepository>();

    private AddFavoriteCommandHandler CreateHandler()
        => new(_currencyRepository, _favoriteRepository);

    private static Currency MakeCurrency(int id = 1)
        => new()
        {
            Id = id,
            Name = "Доллар США",
            Rate = 85.48m
        };

    [Fact]
    public async Task Handle_ValidNewFavorite_AddsAndSaves()
    {
        _currencyRepository.FindByIdAsync(1, default).Returns(MakeCurrency());
        _favoriteRepository.ExistsAsync(10, 1, default).Returns(false);

        await CreateHandler().Handle(new AddFavoriteCommand(10, 1), default);

        await _favoriteRepository.Received(1).AddAsync(
            Arg.Is<UserFavoriteCurrency>(f => f.UserId == 10 && f.CurrencyId == 1),
            default);
        await _favoriteRepository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_CurrencyNotFound_ThrowsKeyNotFound()
    {
        _currencyRepository.FindByIdAsync(999, default).Returns((Currency?)null);

        var act = () => CreateHandler().Handle(new AddFavoriteCommand(10, 999), default);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_AlreadyFavorite_IsIdempotentAndDoesNotSave()
    {
        _currencyRepository.FindByIdAsync(1, default).Returns(MakeCurrency());
        _favoriteRepository.ExistsAsync(10, 1, default).Returns(true);

        await CreateHandler().Handle(new AddFavoriteCommand(10, 1), default);

        await _favoriteRepository.DidNotReceive().AddAsync(Arg.Any<UserFavoriteCurrency>(), default);
        await _favoriteRepository.DidNotReceive().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_CurrencyNotFound_DoesNotAdd()
    {
        _currencyRepository.FindByIdAsync(999, default).Returns((Currency?)null);

        try
        {
            await CreateHandler().Handle(new AddFavoriteCommand(10, 999), default);
        }
        catch (KeyNotFoundException)
        {
        }

        await _favoriteRepository.DidNotReceive().AddAsync(Arg.Any<UserFavoriteCurrency>(), default);
    }
}
