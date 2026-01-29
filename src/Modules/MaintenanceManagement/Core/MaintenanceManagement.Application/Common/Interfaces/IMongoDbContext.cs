using MongoDB.Driver;

namespace MaintenanceManagement.Application.Common.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}
