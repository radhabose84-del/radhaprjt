using Core.Application.Common.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, string databaseName)
        {
            if (client is null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException(nameof(databaseName));

            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            EnsureCollectionExists(name);
            return _database.GetCollection<T>(name);
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
        private void EnsureCollectionExists(string collectionName)
        {
            var collectionList = _database.ListCollectionNames().ToList();
            if (!collectionList.Contains(collectionName))
            {
                _database.CreateCollection(collectionName);                
            }
        }
    }
}
