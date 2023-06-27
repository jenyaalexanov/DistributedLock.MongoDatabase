using System;
using System.Threading.Tasks;
using DistributedLock.MongoDatabase.Constants;

namespace DistributedLock.MongoDatabase
{
    public interface IDistributedLock
    {
        /// <summary>
        /// Created for lock, use OnLock and OnWait after it. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <param name="waitSeconds">Time in seconds. Minimum 10 seconds. Time, which means how many seconds the record will be stored in the database. The default is 30 seconds. This is enough.</param>
        /// <returns></returns>
        IDistributedLock DoLock(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds);
        
        /// <summary>
        /// Created for lock, use OnLock and OnWait after it. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <param name="waitSeconds">Time in seconds. Minimum 10 seconds. Time, which means how many seconds the record will be stored in the database. The default is 30 seconds. This is enough.</param>
        /// <returns></returns>
        IDistributedLock DoLock(Guid lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds);
        
        /// <summary>
        /// Called to await a method, use OnWait after it. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="doWorkDelegate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDistributedLock OnLock<T>(Func<Task<T>> doWorkDelegate);
        
        /// <summary>
        /// Called to await a method, use OnWait after it. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="doWorkDelegate"></param>
        /// <returns></returns>
        IDistributedLock OnLock(Func<Task> doWorkDelegate);
        
        /// <summary>
        /// Called if we can use OnLock right now. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="waitDelegate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDistributedLock OnWait<T>(Func<Task<T>> waitDelegate);
        
        /// <summary>
        /// Called if we can use OnLock right now. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="waitDelegate"></param>
        /// <returns></returns>
        IDistributedLock OnWait(Func<Task> waitDelegate);
        
        /// <summary>
        /// Called if we can use OnLock right now. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="waitDelegate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDistributedLock OnWait<T>(Func<T> waitDelegate);

        /// <summary>
        /// Called if we can use OnLock right now. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="waitDelegate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDistributedLock OnWait<T>(Func<int, Task<T>> waitDelegate);

        /// <summary>
        /// Called if we can use OnLock right now. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="waitDelegate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDistributedLock OnWait<T>(Func<int, T> waitDelegate);

        /// <summary>
        /// Called if we can use OnLock right now. Don't forget about ExecuteAsync to execute the command.
        /// </summary>
        /// <param name="waitDelegate"></param>
        /// <returns></returns>
        IDistributedLock OnWait(Func<int, Task> waitDelegate);
        
        /// <summary>
        /// ExecuteAsync used for execute the command.
        /// </summary>
        /// <typeparam name="T">Your return type</typeparam>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>();
        
        /// <summary>
        /// ExecuteAsync used for execute the command.
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();

        /// <summary>
        /// Start locking by Id
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <param name="waitSeconds">Time in seconds. Minimum 10 seconds. Time, which means how many seconds the record will be stored in the database. The default is 30 seconds. This is enough.</param>
        /// <returns></returns>
        Task<bool> AcquireLockAsync(Guid lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds);

        /// <summary>
        /// Start locking by Id
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <param name="waitSeconds">Time in seconds. Minimum 10 seconds. Time, which means how many seconds the record will be stored in the database. The default is 30 seconds. This is enough.</param>
        /// <returns></returns>
        Task<bool> AcquireLockAsync(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds);

        /// <summary>
        /// Start locking by Id with using
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <param name="waitSeconds">Time in seconds. Minimum 10 seconds. Time, which means how many seconds the record will be stored in the database. The default is 30 seconds. This is enough.</param>
        /// <returns></returns>
        Task<DistributedLockHandle> AcquireUsingLockAsync(string lockId,
            int waitSeconds = CommonConstants.DefaultWaitSeconds);
        
        /// <summary>
        /// Start locking by Id with using
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <param name="waitSeconds">Time in seconds. Minimum 10 seconds. Time, which means how many seconds the record will be stored in the database. The default is 30 seconds. This is enough.</param>
        /// <returns></returns>
        Task<DistributedLockHandle> AcquireUsingLockAsync(Guid lockId,
            int waitSeconds = CommonConstants.DefaultWaitSeconds);

        /// <summary>
        /// Start locking by Id. This is the tuple, where it returns whether we were able to make a lock and if false, then how many seconds to wait.
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <param name="waitSeconds">Time in seconds. Minimum 10 seconds. Time, which means how many seconds the record will be stored in the database. The default is 30 seconds. This is enough.</param>
        /// <returns>Return tuple (bool, int) with: were we able to make a lock(bool) and how many seconds to wait if false(int)</returns>
        Task<(bool, int)> AcquireTupleLockAsync(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds);

        /// <summary>
        /// Release lock by Id
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <returns></returns>
        Task ReleaseLockAsync(Guid lockId);

        /// <summary>
        /// Release lock by Id
        /// </summary>
        /// <param name="lockId">Your lock identifier</param>
        /// <returns></returns>
        Task ReleaseLockAsync(string lockId);
    }
}