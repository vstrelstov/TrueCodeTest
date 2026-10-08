namespace TrueCodeTest.Shared.Entities;

public sealed class Currency
{
    public int Id { get; set; }

    /// <summary>Название валюты в том виде, в каком его публикует ЦБ РФ (например, «Доллар США»).</summary>
    public string Name { get; set; } = null!;

    /// <summary>Курс одной единицы валюты в рублях.</summary>
    public decimal Rate { get; set; }

    /// <summary>Пользователи, добавившие валюту в избранное.</summary>
    public ICollection<UserFavoriteCurrency> FavoritedBy { get; set; } = [];
}
