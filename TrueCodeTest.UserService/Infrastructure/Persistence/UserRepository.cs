using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Data;
using TrueCodeTest.Shared.Entities;
using TrueCodeTest.UserService.Domain.Interfaces;

namespace TrueCodeTest.UserService.Infrastructure.Persistence;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> FindByNameAsync(string name, CancellationToken cancellationToken)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Name == name, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        => dbContext.Users.AnyAsync(u => u.Name == name, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await dbContext.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
