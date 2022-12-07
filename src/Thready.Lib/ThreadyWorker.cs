namespace Thready.Lib;

/// <summary>
/// Worker that splits of a thread in the background and executes a given function with the given delay.
/// </summary>
public class ThreadyWorker
{
    private PeriodicTimer? _timer;
    private Task? _task;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Asynchronously starts this worker with a background thread and executes the given <paramref name="func"/>
    /// with the <paramref name="delay"/>
    /// </summary>
    /// <param name="func"><see cref="Func{TResult}"/> that should be executed by this worker.</param>
    /// <param name="delay"><see cref="TimeSpan"/> in which the <paramref name="func"/> should be executed.</param>
    public async Task StartAsync(Func<Task> func, TimeSpan delay)
    {
        await _semaphore.WaitAsync();
        try
        {
            _timer = new PeriodicTimer(delay);
            _task = Task.Run(async () =>
            {
                while (await _timer.WaitForNextTickAsync())
                {
                    await func();
                }
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Asynchronously stops this worker.
    /// </summary>
    public async Task StopAsync()
    {
        _timer?.Dispose();

        if (_task is not null)
        {
            await _task;
        }
    }
}