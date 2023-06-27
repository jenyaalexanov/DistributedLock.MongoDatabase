using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using DistributedLock.MongoDatabase.Constants;
using DistributedLock.MongoDatabase.Interfaces;
using MongoDB.Driver;

namespace DistributedLock.MongoDatabase.Contexts
{
    internal sealed class DistributedLockDbContext : IDistributedLockDbContext
    {
        private readonly IMongoCollection<BsonDocument> _locksCollection;

        private static FindOneAndUpdateOptions<BsonDocument> BeforeOption => 
            new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.Before
            };
        
        private static FindOneAndUpdateOptions<BsonDocument> AfterOption => 
            new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

        public DistributedLockDbContext(string connection, string databaseName)
        {
            var client = new MongoClient(connection);
            var database = client.GetDatabase(databaseName);
            _locksCollection = database.GetCollection<BsonDocument>(CommonConstants.MongoDbLocksCollectionField);
            
            var filter = new BsonDocument("name", CommonConstants.MongoDbLocksCollectionField);
            var collections = database.ListCollectionNames(new ListCollectionNamesOptions { Filter = filter });
            if (!collections.Any())
            {
                database.CreateCollection(CommonConstants.MongoDbLocksCollectionField);
                _locksCollection.Indexes.CreateOne(
                    new CreateIndexModel<BsonDocument>(
                        Builders<BsonDocument>.IndexKeys.Ascending(CommonConstants.MongoDbCreatedAtField),
                        new CreateIndexOptions { ExpireAfter = TimeSpan.FromSeconds(CommonConstants.DefaultMinWaitSeconds) }
                    )
                );
            }
        }

        public async Task<(bool, int)> AcquireTupleLockAsync(string lockId,
            int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            // waitSeconds must be 10 second or more
            if (waitSeconds < CommonConstants.DefaultMinWaitSeconds)
                waitSeconds = CommonConstants.DefaultMinWaitSeconds;
            
            var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.MongoDbIdField, lockId);
            var update = Builders<BsonDocument>.Update
                .Set(CommonConstants.MongoDbLockField, true)
                .SetOnInsert(CommonConstants.MongoDbCreatedAtField, DateTime.UtcNow.AddSeconds(waitSeconds-CommonConstants.DefaultMinWaitSeconds));
            var updateSeconds = Builders<BsonDocument>.Update
                .Set(CommonConstants.MongoDbCreatedAtField, DateTime.UtcNow.AddSeconds(waitSeconds-CommonConstants.DefaultMinWaitSeconds));
            
            var result = await _locksCollection.FindOneAndUpdateAsync(filter, update, BeforeOption);
            var isLockedBefore = result?.GetValue(CommonConstants.MongoDbLockField).AsBoolean ?? false;
            
            if (!isLockedBefore)
            {
                await _locksCollection.FindOneAndUpdateAsync(filter, updateSeconds);
                return (true, 0);
            }

            var dbDateTime = result.GetValue(CommonConstants.MongoDbCreatedAtField)
                .AsBsonDateTime
                .ToUniversalTime();
            var howintWaitSeconds = (int)(dbDateTime.AddSeconds(CommonConstants.DefaultMinWaitSeconds) - DateTime.UtcNow)
                .TotalSeconds;

            return (false, howintWaitSeconds);
        }
        
        public async Task ReleaseLockAsync(string lockId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.MongoDbIdField, lockId);
            var update = Builders<BsonDocument>.Update.Set(CommonConstants.MongoDbLockField, false);

            await _locksCollection.UpdateOneAsync(filter, update);
        }
    }
}