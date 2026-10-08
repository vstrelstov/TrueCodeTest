namespace TrueCodeTest.UserService.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string userName);
}
