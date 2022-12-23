using FluentAssertions;

namespace Thready.Lib.Tests.Integration;

public class ThreadyWorkerTests
{
    [Fact(DisplayName = "Starting Worker that is not already started should periodically execute func")]
    public async Task StartingWorker_WhenNotAlreadyStarted_ShouldPeriodicallyExecute()
    {
        // Arrange
        var counter = 0;
        var sut = new ThreadyWorker((ct) => Task.FromResult(counter++), 
            delay: TimeSpan.FromMilliseconds(10));

        // Act
        await sut.StartAsync();

        // Assert
        await Task.Delay(105);
        counter.Should().Be(10);
    }

    [Fact(DisplayName = "Stopping Worker with completion func defined should execute completion func once and stop")]
    public async Task StoppingWorkerWithCompletionFunc_WhenStarted_ShouldExecuteCompletionFuncOnce()
    {
        // Arrange
        var periodicCounter = 0;
        var completionCounter = 0;
        var sut = new ThreadyWorker((ct) => Task.FromResult(periodicCounter++),
            completionFunc: () => Task.FromResult(completionCounter++),
            delay: TimeSpan.FromMilliseconds(10));
        await sut.StartAsync();
        await Task.Delay(10);

        // Act
        await sut.StopAsync();

        // Assert
        periodicCounter.Should().Be(1);
        completionCounter.Should().Be(1);
    }
    
    [Fact(DisplayName = "Stopping Worker that is not started should throw InvalidOperationException")]
    public async Task StoppingWorker_WhenNotStarted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var periodicCounter = 0;
        var completionCounter = 0;
        var sut = new ThreadyWorker((ct) => Task.FromResult(periodicCounter++),
            completionFunc: () => Task.FromResult(completionCounter++),
            delay: TimeSpan.FromMilliseconds(10));

        // Act
        Func<Task> act = () => sut.StopAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Can not stop a worker that has not yet been started!");
        periodicCounter.Should().Be(0);
        completionCounter.Should().Be(0);
    }
    
    [Fact(DisplayName = "Stopping Worker via force should stop immediately")]
    public async Task StoppingWorker_WhenForceStopped_ShouldStopImmediately()
    {
        // Arrange
        var periodicCounter = 0;
        var completionCounter = 0;
        var sut = new ThreadyWorker((ct) => Task.FromResult(periodicCounter++),
            completionFunc: () => Task.FromResult(completionCounter++),
            delay: TimeSpan.FromMilliseconds(10));
        await sut.StartAsync();
        await Task.Delay(10);

        // Act
        Func<Task> act = () => sut.StopAsync(true);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        await Task.Delay(20);
        periodicCounter.Should().Be(1);
        completionCounter.Should().Be(0);
    }
}