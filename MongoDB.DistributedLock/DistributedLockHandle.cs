using System;
using System.Threading.Tasks;

namespace MongoDB.DistributedLock
{
    public class DistributedLockHandle : IAsyncDisposable
    {
        private readonly string _id;
        private readonly IDistributedLock _distributedLock;

        public string GetId() => _id;

        public DistributedLockHandle(IDistributedLock distributedLock, string id)
        {
            _distributedLock = distributedLock;
            _id = id;
        }
        
        public DistributedLockHandle(IDistributedLock distributedLock, Guid id)
        {
            _distributedLock = distributedLock;
            _id = id.ToString();
        }

        public async ValueTask DisposeAsync()
        {
            await _distributedLock.ReleaseLockAsync(_id);
        }
    }
}