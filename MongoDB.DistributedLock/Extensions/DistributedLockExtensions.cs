using Microsoft.Extensions.DependencyInjection;
using MongoDB.DistributedLock.Contexts;
using MongoDB.DistributedLock.Interfaces;

namespace MongoDB.DistributedLock.Extensions
{
    public static class DistributedLockExtensions
    {
        public static IServiceCollection AddDistributedLock(
            this IServiceCollection service, string mongoDbConnection, string databaseName)
        {
            service.AddScoped<IDistributedLockDbContext>(provider => new DistributedLockDbContext(mongoDbConnection, databaseName));
            service.AddScoped<IDistributedLock, DistributedLock>();
            
            return service;
        }
    }
}