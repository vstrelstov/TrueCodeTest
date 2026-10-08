namespace TrueCodeTest.Shared.Entities;

public sealed class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>
    /// Хеш пароля. Исходный пароль не хранится ни в каком виде.
    /// </summary>
    public string Password { get; set; } = null!;

    public ICollection<UserFavoriteCurrency> Favorites { get; set; } = [];
}
