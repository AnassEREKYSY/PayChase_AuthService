using Core.Entities;
using Infrastructure.IRepositories;
using MongoDB.Driver;


namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _col;
    public UserRepository(IMongoDatabase db) => _col = db.GetCollection<User>("users");

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        await _col.Find(x => x.Email == email).FirstOrDefaultAsync(ct);

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct) =>
        await _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<string> CreateAsync(User user, CancellationToken ct)
    {
        await _col.InsertOneAsync(user, cancellationToken: ct);
        return user.Id;
    }
}
