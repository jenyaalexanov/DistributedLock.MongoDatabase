using System.Threading.Tasks;
using MongoDB.DistributedLock.Constants;

namespace MongoDB.DistributedLock.Interfaces
{
    public interface IDistributedLockDbContext
    {
        Task<(bool, int)> AcquireTupleLockAsync(string lockId, int waitSeconds = CommonConstants.DefaultWaitSeconds);

        Task ReleaseLockAsync(string lockId);
    }
}