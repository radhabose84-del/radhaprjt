using MongoDB.Driver;

namespace QCManagement.Application.Common.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
        IMongoDatabase GetDatabase();
    }
}
