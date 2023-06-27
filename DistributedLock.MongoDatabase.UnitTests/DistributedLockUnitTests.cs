using DistributedLock.MongoDatabase.Interfaces;
using Moq;

namespace DistributedLock.MongoDatabase.UnitTests;

public class DistributedLockUnitTests
{
    private Mock<IDistributedLockDbContext> _mockMongoDbContext;
    private IDistributedLock _distributedLock;

    [SetUp]
    public void SetUp()
    {
        _mockMongoDbContext = new Mock<IDistributedLockDbContext>();
        _distributedLock = new DistributedLock(_mockMongoDbContext.Object);
    }

    [Test]
    public async Task ExecuteAsync_WhenLockAcquired_ExecutesDoWorkDelegate()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((true, 1));
        bool wasDoWorkCalled = false;
        Func<Task> doWorkDelegate = () => { wasDoWorkCalled = true; return Task.CompletedTask; };

        _distributedLock.DoLock(lockId).OnLock(doWorkDelegate);

        // Act
        await _distributedLock.ExecuteAsync();

        // Assert
        Assert.IsTrue(wasDoWorkCalled);
        _mockMongoDbContext.Verify(m => m.ReleaseLockAsync(lockId), Times.Once);
    }
    
    [Test]
    public async Task ExecuteAsync_WhenLockNotAcquired_ExecutesWaitDelegate()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((false, 1));
        bool wasWaitCalled = false;
        Func<Task> waitDelegate = () => { wasWaitCalled = true; return Task.CompletedTask; };

        _distributedLock.DoLock(lockId).OnWait(waitDelegate);

        // Act
        await _distributedLock.ExecuteAsync();

        // Assert
        Assert.IsTrue(wasWaitCalled);
        _mockMongoDbContext.Verify(m => m.ReleaseLockAsync(lockId), Times.Never);
    }
    
    [Test]
    public async Task AcquireLockAsync_WhenLockIsAcquired_ReturnsTrue()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((true, 1));

        // Act
        var result = await _distributedLock.AcquireLockAsync(lockId);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task AcquireLockAsync_WhenLockIsNotAcquired_ReturnsFalse()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((false, 1));

        // Act
        var result = await _distributedLock.AcquireLockAsync(lockId);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task AcquireUsingLockAsync_WhenLockIsAcquired_ReturnsLockHandle()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((true, 1));

        // Act
        var result = await _distributedLock.AcquireUsingLockAsync(lockId);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.GetId(), Is.EqualTo(lockId));
    }

    [Test]
    public async Task AcquireUsingLockAsync_WhenLockIsNotAcquired_ReturnsNull()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((false, 1));

        // Act
        var result = await _distributedLock.AcquireUsingLockAsync(lockId);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task ReleaseLockAsync_WhenCalled_ReleasesLock()
    {
        // Arrange
        string lockId = "testLock";

        // Act
        await _distributedLock.ReleaseLockAsync(lockId);

        // Assert
        _mockMongoDbContext.Verify(m => m.ReleaseLockAsync(lockId), Times.Once);
    }
    
    [Test]
    public async Task ExecuteAsync_WhenLockAcquired_ExecutesDoWorkDelegateWithReturnValue()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync((true, 1));
        Func<Task<int>> doWorkDelegate = () => Task.FromResult(42);

        _distributedLock.DoLock(lockId).OnLock(doWorkDelegate);

        // Act
        var result = await _distributedLock.ExecuteAsync<int>();

        // Assert
        Assert.That(result, Is.EqualTo(42));
        _mockMongoDbContext.Verify(m => m.ReleaseLockAsync(lockId), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenLockNotAcquired_ExecutesWaitDelegateWithReturnValue()
    {
        // Arrange
        string lockId = "testLock";
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync((false, 1));
        Func<Task<int>> waitDelegate = () => Task.FromResult(24);

        _distributedLock.DoLock(lockId).OnWait(waitDelegate);

        // Act
        var result = await _distributedLock.ExecuteAsync<int>();

        // Assert
        Assert.That(result, Is.EqualTo(24));
        _mockMongoDbContext.Verify(m => m.ReleaseLockAsync(lockId), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenLockNotAcquired_ExecutesWaitDelegateWithDelay()
    {
        // Arrange
        string lockId = "testLock";
        int delay = 15;
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync((false, delay));
        // Act
        var (result, seconds) =await _distributedLock.AcquireTupleLockAsync(lockId, delay);

        // Assert
        Assert.That(seconds, Is.EqualTo(delay));
        Assert.That(result, Is.EqualTo(false));
        _mockMongoDbContext.Verify(m => m.ReleaseLockAsync(lockId), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenLockAcquired_ExecutesDoWorkDelegateWithGuidLock()
    {
        // Arrange
        Guid lockId = Guid.NewGuid();
        _mockMongoDbContext.Setup(m => m.AcquireTupleLockAsync(It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync((true, 1));
        bool wasDoWorkCalled = false;
        Func<Task> doWorkDelegate = () => { wasDoWorkCalled = true; return Task.CompletedTask; };

        _distributedLock.DoLock(lockId).OnLock(doWorkDelegate);

        // Act
        await _distributedLock.ExecuteAsync();

        // Assert
        Assert.IsTrue(wasDoWorkCalled);
        _mockMongoDbContext.Verify(m => m.ReleaseLockAsync(lockId.ToString()), Times.Once);
    }
}