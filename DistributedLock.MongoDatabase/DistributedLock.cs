using System;
using System.Threading.Tasks;
using DistributedLock.MongoDatabase.Constants;
using DistributedLock.MongoDatabase.Interfaces;

namespace DistributedLock.MongoDatabase
{
    public class DistributedLock : IDistributedLock
    {
        private readonly IDistributedLockDbContext _mongoDbContext;
        
        private string _lockId;
        private int _waitSeconds;
        private dynamic _doWorkDelegate;
        private dynamic _waitDelegate;

        public DistributedLock(IDistributedLockDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }
        
        public IDistributedLock DoLock(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            _lockId = lockId;
            _waitSeconds = waitSeconds;
            return this;
        }

        public IDistributedLock DoLock(Guid lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            _lockId = lockId.ToString();
            _waitSeconds = waitSeconds;
            return this;
        }
        
        public IDistributedLock OnLock<T>(Func<Task<T>> doWorkDelegate)
        {
            _doWorkDelegate = doWorkDelegate;
            return this;
        }
        
        public IDistributedLock OnWait<T>(Func<Task<T>> waitDelegate)
        {
            _waitDelegate = waitDelegate;
            return this;
        }
        
        public IDistributedLock OnWait<T>(Func<int, Task<T>> waitDelegate)
        {
            _waitDelegate = waitDelegate;
            return this;
        }
        
        public IDistributedLock OnWait<T>(Func<int, T> waitDelegate)
        {
            _waitDelegate = waitDelegate;
            return this;
        }
        
        public IDistributedLock OnWait<T>(Func<T> waitDelegate)
        {
            _waitDelegate = waitDelegate;
            return this;
        }

        public IDistributedLock OnLock(Func<Task> doWorkDelegate)
        {
            _doWorkDelegate = doWorkDelegate;
            return this;
        }

        public IDistributedLock OnWait(Func<Task> waitDelegate)
        {
            _waitDelegate = waitDelegate;
            return this;
        }
        
        public IDistributedLock OnWait(Func<int, Task> waitDelegate)
        {
            _waitDelegate = waitDelegate;
            return this;
        }

        public async Task<T> ExecuteAsync<T>()
        {
            var (acquire, x) = await AcquireTupleLockAsync(_lockId, _waitSeconds);
            if (!acquire)
            {
                switch (_waitDelegate)
                {
                    case Func<T> typedDelegate:
                        return typedDelegate();
                    case Func<Task<T>> taskTypedDelegate:
                        return await taskTypedDelegate();
                    case Func<Task> taskTypedDelegate:
                        await taskTypedDelegate();
                        return default;
                    case Func<int, Task<T>> waitDelegate:
                        return await waitDelegate(x);
                    case Func<int, T> waitDelegate:
                        return waitDelegate(x);
                    default:
                        _waitDelegate();
                        return default;
                }
            }

            try
            {
                switch (_doWorkDelegate)
                {
                    case Func<T> typedWorkDelegate:
                        return typedWorkDelegate();
                    case Func<Task<T>> typedWorkDelegate:
                        return await typedWorkDelegate();
                    case Func<Task> workDelegate:
                        await workDelegate();
                        return default;
                    default:
                        await _doWorkDelegate();
                        return default;
                }
            }
            finally
            {
                await ReleaseLockAsync(_lockId);
            }
        }

        public async Task ExecuteAsync()
        {
            var (acquire, x) = await AcquireTupleLockAsync(_lockId, _waitSeconds);
            if (!acquire)
            {
                switch (_waitDelegate)
                {
                    case Func<Task> taskTypedDelegate:
                        await taskTypedDelegate();
                        return;
                    case Func<int, Task> waitDelegate:
                        await waitDelegate(x);
                        return;
                    default:
                        await _waitDelegate();
                        return;
                }
            }

            try
            {
                switch (_doWorkDelegate)
                {
                    case Func<Task> workDelegate:
                        await workDelegate();
                        return;
                    default:
                        await _doWorkDelegate();
                        return;
                }
            }
            finally
            {
                await ReleaseLockAsync(_lockId);
            }
        }

        public async Task<bool> AcquireLockAsync(Guid lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            var result = await AcquireTupleLockAsync(lockId.ToString(), waitSeconds);
            return result.Item1;
        }
        
        public async Task<DistributedLockHandle> AcquireUsingLockAsync(string id, int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            if (await AcquireLockAsync(id, waitSeconds))
            {
                return new DistributedLockHandle(this, id);
            }

            return null;
        }
        
        public async Task<DistributedLockHandle> AcquireUsingLockAsync(Guid id, int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            if (await AcquireLockAsync(id, waitSeconds))
            {
                return new DistributedLockHandle(this, id);
            }

            return null;
        }

        public async Task<bool> AcquireLockAsync(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            var result = await AcquireTupleLockAsync(lockId, waitSeconds);
            return result.Item1;
        }
        
        public async Task ReleaseLockAsync(Guid lockId)
        {
            await ReleaseLockAsync(lockId.ToString());
        }

        public Task<(bool, int)> AcquireTupleLockAsync(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds)
        {
            return _mongoDbContext.AcquireTupleLockAsync(lockId, waitSeconds);
        }

        public Task ReleaseLockAsync(string lockId)
        {
            return _mongoDbContext.ReleaseLockAsync(lockId);
        }
    }
}