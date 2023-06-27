using System.Threading.Tasks;
using DistributedLock.MongoDatabase.Constants;

namespace DistributedLock.MongoDatabase.Interfaces
{
    public interface IDistributedLockDbContext
    {
        Task<(bool, int)> AcquireTupleLockAsync(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds);

        Task ReleaseLockAsync(string lockId);
    }
}