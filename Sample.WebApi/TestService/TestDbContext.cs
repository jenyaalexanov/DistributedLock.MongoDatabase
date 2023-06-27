using MongoDB.Driver;
using Sample.WebApi.TestService.TestModels;

namespace Sample.WebApi.TestService;

public class TestDbContext
{
    private readonly IMongoDatabase _database;

    public TestDbContext()
    {
        var client = new MongoClient("only for test");
        _database = client.GetDatabase("only for test");
    }

    public IMongoCollection<TestModel> TestModels => 
        _database.GetCollection<TestModel>("test_model");
}