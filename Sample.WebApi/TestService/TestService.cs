using MongoDB.Driver;
using Sample.WebApi.TestService.TestModels;

namespace Sample.WebApi.TestService;

public record Dto(string name);

public class TestService
{
    private readonly TestDbContext _testDbContext;

    public TestService()
    {
        _testDbContext = new TestDbContext();
    }

    public async Task<IEnumerable<TestModel>> GetAll()
    {
        return await _testDbContext.TestModels.Find(_ => true).ToListAsync();
    }
    
    public async Task<TestModel?> Get(string id)
    {
        return await _testDbContext.TestModels.Find(x => x.Id == id).FirstOrDefaultAsync();
    }
    
    public async Task Post(Dto dto)
    {
        var testModel = new TestModel()
        {
            Name = dto.name,
            CreatedAt = DateTime.UtcNow
        };
        
        await _testDbContext.TestModels.InsertOneAsync(testModel);
    }
    
    public async Task Put(string id, Dto dto)
    {
        var update = Builders<TestModel>.Update
            .Set(x => x.Name, dto.name)
            .Set(x => x.UpdateAt, DateTime.UtcNow)
            .Set(x => x.SomeUpdateString, $"BB_{dto.name}_bb")
            .Set(x => x.SomeUpdateNumber, 95);
        await _testDbContext.TestModels.UpdateOneAsync(x => x.Id == id, update);
    }
    
    public async Task<string> PutWithMessage(string id, Dto dto)
    {
        await Put(id, dto);
        return dto.name;
    }
    
    public async Task Delete(string id)
    {
        await _testDbContext.TestModels.DeleteOneAsync(x => x.Id == id);
    }
}