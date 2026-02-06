using MongoDB.Driver;

namespace WarehouseManagement.Application.Common.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}
