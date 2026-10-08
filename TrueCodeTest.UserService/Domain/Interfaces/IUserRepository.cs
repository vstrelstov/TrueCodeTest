using TrueCodeTest.Shared.Entities;

namespace TrueCodeTest.UserService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByNameAsync(string name, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
