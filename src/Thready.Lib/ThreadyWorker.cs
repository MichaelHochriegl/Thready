namespace Thready.Lib;

/// <summary>
/// Worker that splits of a thread in the background and executes a given function with the given delay.
/// </summary>
public class ThreadyWorker
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Func<CancellationToken, Task> _periodicFunc;
    private readonly Func<Task>? _completionFunc;
    private readonly TimeSpan _delay;
    private PeriodicTimer? _timer;
    private Task? _task;
    private CancellationTokenSource? _cts;

    public ThreadyWorker(Func<CancellationToken, Task> periodicFunc, Func<Task>? completionFunc = null, TimeSpan? delay = null)
    {
        _periodicFunc = periodicFunc;
        _completionFunc = completionFunc;
        _delay = delay ?? TimeSpan.FromSeconds(1);
    }
    
    /// <summary>
    /// Asynchronously starts this worker with a background thread and executes the operation with periodic with the set delay.
    /// </summary>
    public async Task StartAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_timer is null && _cts is null)
            {
                _timer = new PeriodicTimer(_delay);
                _cts = new CancellationTokenSource();
                _task = Task.Run(async () =>
                {
                    while (await _timer.WaitForNextTickAsync(_cts.Token))
                    {
                        await _periodicFunc(_cts.Token);
                    }
                });
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Asynchronously stops this worker.
    /// </summary>
    public async Task StopAsync(bool forceStop = false)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_task is null)
            {
                throw new InvalidOperationException("Can not stop a worker that has not yet been started!");
            }

            if (forceStop)
            {
                _cts?.Cancel();
            }
            
            _timer?.Dispose();

            if (_task is not null)
            {
                await _task;
            }

            if (_completionFunc is not null)
            {
                await _completionFunc();
            }
        }
        finally
        {
            _cts?.Dispose();
            _semaphore.Release();
        }
    }
}