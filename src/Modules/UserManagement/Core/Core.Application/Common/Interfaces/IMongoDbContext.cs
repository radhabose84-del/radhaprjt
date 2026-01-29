using MongoDB.Driver;

namespace Core.Application.Common.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}
