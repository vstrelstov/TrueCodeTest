namespace TrueCodeTest.UserService.Infrastructure.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = null!;

    public string Issuer { get; set; } = "TrueCodeTest.UserService";

    public string Audience { get; set; } = "TrueCodeTest";

    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(1);
}
