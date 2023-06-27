namespace DistributedLock.MongoDatabase.Constants
{
    internal static class CommonConstants
    {
        internal const string MongoDbLocksCollectionField = "_locks";
        
        internal const string MongoDbIdField = "_id";
        
        internal const string MongoDbLockField = "lock";
        
        internal const string MongoDbCreatedAtField = "createdAt";

        internal const int DefaultWaitSeconds = 30;

        internal const int DefaultMinWaitSeconds = 10;
    }
}