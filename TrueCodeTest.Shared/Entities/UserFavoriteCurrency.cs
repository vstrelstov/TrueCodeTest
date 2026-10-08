namespace TrueCodeTest.Shared.Entities;

public sealed class UserFavoriteCurrency
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int CurrencyId { get; set; }

    public Currency Currency { get; set; } = null!;
}
