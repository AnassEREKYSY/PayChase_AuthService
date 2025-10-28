using Core.Entities;
using Infrastructure.IRepositories;
using MongoDB.Driver;

namespace Infrastructure.Repositories;
public class OrgRepository : IOrgRepository
{
    private readonly IMongoCollection<Org> _col;
    public OrgRepository(IMongoDatabase db) => _col = db.GetCollection<Org>("auth.orgs");
    public Task<Org?> GetByIdAsync(string id, CancellationToken ct) =>
        _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
}