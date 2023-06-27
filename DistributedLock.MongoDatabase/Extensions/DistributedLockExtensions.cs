using Microsoft.Extensions.DependencyInjection;
using DistributedLock.MongoDatabase.Contexts;
using DistributedLock.MongoDatabase.Interfaces;

namespace DistributedLock.MongoDatabase.Extensions
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