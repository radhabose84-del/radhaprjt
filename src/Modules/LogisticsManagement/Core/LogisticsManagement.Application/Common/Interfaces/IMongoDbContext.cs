using MongoDB.Driver;

namespace LogisticsManagement.Application.Common.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}
