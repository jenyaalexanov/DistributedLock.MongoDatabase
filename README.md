# DistributedLock.MongoDatabase

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/DistributedLock.MongoDatabase?style=for-the-badge)
![Nuget](https://img.shields.io/nuget/dt/DistributedLock.MongoDatabase?style=for-the-badge)
[![GitHub license](https://img.shields.io/github/license/jenyaalexanov/DistributedLock.MongoDatabase?style=for-the-badge)](https://github.com/jenyaalexanov/DistributedLock.MongoDatabase/blob/master/LICENSE)

<img src="https://github.com/jenyaalexanov/DistributedLock.MongoDatabase/blob/master/DistributedLock.MongoDatabase/NugetLogo.png" alt="MongoDB DistributedLock Logo" width=128 height=128 />

- DistributedLock.MongoDatabase is a library that allows you not to worry about multiple instances and easily block sections of code by identifier.
- Based on dotnet standard 2.1, dotnet standard 2.0
- The library itself will create the right table for locks and mongoDB itself will delete records at the right time. You don't need to do anything else yourself.

>I wrote this library for myself, but I'm sharing it with the community because it can help other developers. 
Now I use serverless for my projects and had some problems with several instances, and lambdas that can fall asleep at any time.
That's what led me to make the DistributedLock.MongoDatabase library.

>I searched for different solutions, but didn't find anything good for mongoDB.
>I tried serverless redis, but it wasn't quite what I needed. I needed a good solution for mongoDB and I made one.

_I'm always open to your changes, commits and wishes :)_

How do I get started?
--------------
Add DistributedLock.MongoDatabase to your project:

**Package Manager**

	PM> NuGet\Install-Package DistributedLock.MongoDatabase -Version 3.1.1
  
**.NET CLI**

	>dotnet add package DistributedLock.MongoDatabase --version 3.1.1
  
**or something else**

--------------


You only need to add AddDistributedLock to your application

    builder.Services.AddDistributedLock(
        "your mongodb connection string", 
        "your mongodb database name"
    );
After that you can inject it anywhere in the project, here's an example using the DI
    
    private readonly IDistributedLock _distributedLock;
    
    public LockController(IDistributedLock distributedLock)
    {
        _distributedLock = distributedLock;
    }

You can use different variations of your lock. For example, like this:

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

Or with using

    await using (var handler = await _distributedLock.AcquireUsingLockAsync(id))
    {
        if(handler == null)
           throw new Exception("Locked");

        await _testService.Put(id, dto);
    }

Or even using fluent

    await _distributedLock.DoLock(id)
        .OnLock(async () => await _testService.Put(id, dto))
        .OnWait(() => throw new Exception("Locked"))
        .ExecuteAsync();

You can return any type

    return await _distributedLock.DoLock(id)
        .OnLock(async () => await _testService.PutWithMessage(id, dto))
        .OnWait(async x=> await SomeStringInfo(x))
        .ExecuteAsync<string>();

You can use AcquireTupleLockAsync to get tuple with waitSeconds, to understand 
if somehow the instance dies, or we somehow don't get a ReleaseLockAsync, 
then the result will be false and the time in seconds (how long we should wait).
By default it's 30 seconds.

    var (acquireLock, waitSeconds) = await _distributedLock.AcquireTupleLockAsync("PassHereUniqueIdentifier");

    if (!acquireLock)
        throw new Exception($"Locked. You need to wait: {waitSeconds} sec.");

    try
    {
        await _testService.Post(dto);
    }
    finally
    {
        await _distributedLock.ReleaseLockAsync("PassHereUniqueIdentifier");
    }

You can find **all these examples** and **work with them** in the [Sample.WebApi](https://github.com/jenyaalexanov/DistributedLock.MongoDatabase/tree/master/Sample.WebApi)
