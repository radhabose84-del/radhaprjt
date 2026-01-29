using MongoDB.Driver;

namespace FAM.Application.Common.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}
