using Core.Entities;
using Infrastructure.IRepositories;
using MongoDB.Driver;


namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _col;
    public UserRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<User>("auth.users");
    }

    public Task<User?> GetByEmailAsync(string orgId, string email, CancellationToken ct) =>
        _col.Find(x => x.OrgId == orgId && x.Email == email).FirstOrDefaultAsync(ct);

    public Task<User?> GetByIdAsync(string id, CancellationToken ct) =>
        _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<string> CreateAsync(User user, CancellationToken ct)
    {
        await _col.InsertOneAsync(user, cancellationToken: ct);
        return user.Id;
    }
}