using Microsoft.AspNetCore.Mvc;
using MongoDB.DistributedLock;
using Sample.WebApi.TestService;
using Sample.WebApi.TestService.TestModels;

namespace Sample.WebApi.Controllers;

/// <summary>
/// Used only for testing
/// </summary>
[ApiController]
[Route("[controller]")]
public class LockController : ControllerBase
{
    private readonly TestService.TestService _testService = new();

    private readonly IDistributedLock _distributedLock;
    
    public LockController(IDistributedLock distributedLock )
    {
        _distributedLock = distributedLock;
    }
    
    [HttpPost("with-lock")]
    public async Task PostWithLock([FromBody] Dto dto)
    {
        var acquireLock = await _distributedLock.AcquireLockAsync("PassHereUniqueIdentifier");

        if (!acquireLock)
            throw new Exception("Locked");

        try
        {
            await _testService.Post(dto);
        }
        finally
        {
            await _distributedLock.ReleaseLockAsync("PassHereUniqueIdentifier");
        }
    }
    
    [HttpPut("with-lock/{id}")]
    public async Task PutWithLock(string id, [FromBody] Dto dto)
    {
        var acquireLock = await _distributedLock.AcquireLockAsync(id);
        
        if (!acquireLock)
            throw new Exception("Locked");
        
        try
        {
            await _testService.Put(id, dto);
        }
        finally
        {
            await _distributedLock.ReleaseLockAsync(id);
        }
    }
    
    [HttpPut("with-using-lock/{id}")]
    public async Task PutWithUsingLock(string id, [FromBody] Dto dto)
    {
        await using (var handler = await _distributedLock.AcquireUsingLockAsync(id))
        {
            if(handler == null)
                throw new Exception("Locked");
            
            await _testService.Put(id, dto);
        }
    }
    
    [HttpPut("with-using-lock-other/{id}")]
    public async Task PutWithUsingLockOther(string id, [FromBody] Dto dto)
    {
        await using var handler = await _distributedLock.AcquireUsingLockAsync(id);
        if(handler == null)
            throw new Exception("Locked");
            
        await _testService.Put(id, dto);
    }
    
    [HttpPut("with-lock-fluent/{id}")]
    public async Task PutWithLockFluent(string id, [FromBody] Dto dto)
    {
        await _distributedLock.DoLock(id)
            .OnLock(async () => await _testService.Put(id, dto))
            .OnWait(() => throw new Exception("Locked"))
            .ExecuteAsync();
    }
    
    [HttpPut("with-lock-fluent-v2/{id}")]
    public async Task<string> PutWithLockFluentV2(string id, [FromBody] Dto dto)
    {
        return await _distributedLock.DoLock(id)
            .OnLock(async () => await _testService.PutWithMessage(id, dto))
            .OnWait(async x=> await SomeStringInfo(x))
            .ExecuteAsync<string>();
    }

    private async Task<string> SomeStringInfo(int wait)
    {
        // get some async data
        return $"some async data. You need to wait: {wait} sec.";
    }
    
    [HttpPut("with-lock-fluent-v3/{id}")]
    public async Task<string> PutWithLockFluentV3(string id, [FromBody] Dto dto)
    {
        return await _distributedLock.DoLock(id)
            .OnLock(async () => await _testService.PutWithMessage(id, dto))
            .OnWait(SomeStringInfoSync)
            .ExecuteAsync<string>();
    }

    private string SomeStringInfoSync(int wait)
    {
        return $"some data. You need to wait: {wait} sec.";
    }
    
    [HttpGet]
    public async Task<IEnumerable<TestModel>> Get()
    {
        return await _testService.GetAll();
    }
    
    [HttpGet("{id}")]
    public async Task<TestModel?> Get(string id)
    {
        return await _testService.Get(id);
    }
    
    [HttpPost]
    public async Task Post([FromBody] Dto dto)
    {
        await _testService.Post(dto);
    }
    
    [HttpPut("{id}")]
    public async Task Put(string id, [FromBody] Dto dto)
    {
        await _testService.Put(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        await _testService.Delete(id);
    }
}