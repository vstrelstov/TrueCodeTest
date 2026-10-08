using TrueCodeTest.FinanceService.Application.DTOs;
using TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Entities;

namespace TrueCodeTest.FinanceService.Tests.Application.Queries;

public sealed class GetUserCurrenciesQueryHandlerTests
{
    private readonly ICurrencyRepository _currencyRepository = Substitute.For<ICurrencyRepository>();
    private readonly IFavoriteRepository _favoriteRepository = Substitute.For<IFavoriteRepository>();

    private GetUserCurrenciesQueryHandler CreateHandler()
        => new(_currencyRepository, _favoriteRepository);

    private static Currency MakeCurrency(int id, string name, decimal rate)
        => new()
        {
            Id = id, 
            Name = name,
            Rate = rate
        };

    [Fact]
    public async Task Handle_NoFavorites_ReturnsAllCurrencies()
    {
        var all = new List<Currency>
        {
            MakeCurrency(1, "Доллар США", 85.48m),
            MakeCurrency(2, "Евро", 96.33m),
        };
        _favoriteRepository.GetFavoritesAsync(1, default).Returns(new List<Currency>());
        _currencyRepository.GetAllAsync(default).Returns(all);

        var result = await CreateHandler().Handle(new GetUserCurrenciesQuery(1), default);

        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().BeEquivalentTo(["Доллар США", "Евро"]);
    }

    [Fact]
    public async Task Handle_HasFavorites_ReturnsOnlyFavorites()
    {
        var favorites = new List<Currency>
        {
            MakeCurrency(1, "Доллар США", 85.48m),
        };
        _favoriteRepository.GetFavoritesAsync(1, default).Returns(favorites);

        var result = await CreateHandler().Handle(new GetUserCurrenciesQuery(1), default);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Доллар США");
        await _currencyRepository.DidNotReceive().GetAllAsync(default);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectRate()
    {
        _favoriteRepository.GetFavoritesAsync(1, default).Returns([MakeCurrency(1, "Иен", 0.539449m)]);

        var result = await CreateHandler().Handle(new GetUserCurrenciesQuery(1), default);

        result[0].Rate.Should().Be(0.539449m);
    }

    [Fact]
    public async Task Handle_MapsToDtoCorrectly()
    {
        _favoriteRepository.GetFavoritesAsync(1, default).Returns([MakeCurrency(42, "Евро", 96.33m)]);

        var result = await CreateHandler().Handle(new GetUserCurrenciesQuery(1), default);

        result[0].Should().BeEquivalentTo(new CurrencyDto(42, "Евро", 96.33m));
    }

    [Fact]
    public async Task Handle_EmptyCurrencyTable_ReturnsEmpty()
    {
        _favoriteRepository.GetFavoritesAsync(1, default).Returns(new List<Currency>());
        _currencyRepository.GetAllAsync(default).Returns(new List<Currency>());

        var result = await CreateHandler().Handle(new GetUserCurrenciesQuery(1), default);

        result.Should().BeEmpty();
    }
}
