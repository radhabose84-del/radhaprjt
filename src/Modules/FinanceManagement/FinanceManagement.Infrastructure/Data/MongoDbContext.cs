using FinanceManagement.Application.Common.Interfaces;
using MongoDB.Driver;

namespace FinanceManagement.Infrastructure.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient mongoClient, string databaseName)
        {
            _database = mongoClient.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }
}
