namespace Thready.Lib;

/// <summary>
/// A <see cref="ThreadyWorker"/> allows for periodic execution of an asynchronous operation in a background thread.
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

    /// <summary>
    /// Creates an instance of <see cref="ThreadyWorker"/>.
    /// </summary>
    /// <param name="periodicFunc">The async operation to periodically execute.
    /// This operation can support immediate cancellation by providing a <see cref="CancellationToken"/> to it.</param>
    /// <param name="completionFunc">The async operation that shall be executed before stopping the <see cref="ThreadyWorker"/>.
    /// This operation does NOT support immediate cancellation!
    /// It will NOT be executed when the <paramref name="periodicFunc"/> throws an exception during cancellation!</param>
    /// <param name="delay">The interval for the periodic execution.
    /// NOTE: If your <paramref name="periodicFunc"/> takes longer then the <paramref name="delay"/> it will execute right away!</param>
    public ThreadyWorker(Func<CancellationToken, Task> periodicFunc, Func<Task>? completionFunc = null, TimeSpan? delay = null)
    {
        _periodicFunc = periodicFunc;
        _completionFunc = completionFunc;
        _delay = delay ?? TimeSpan.FromSeconds(1);
    }
    
    /// <summary>
    /// Asynchronously starts this worker with a background thread and executes the operation periodic with the set delay.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous start of the background task.</returns>
    /// <remarks>The returned <see cref="Task"/> does NOT represent the asynchronous execution of the given operation!</remarks>
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
    /// <param name="forceStop">If true, the operation will be force-cancelled via the <see cref="CancellationToken"/> passed to it.
    /// Otherwise the current running execution of the operation will be completed before stopping the periodic worker.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous stopping of the background task.</returns>
    /// <exception cref="InvalidOperationException">If <see cref="StartAsync"/> has not yet been called for this <see cref="ThreadyWorker"/>.</exception>
    /// <exception cref="OperationCanceledException">If <paramref name="forceStop"/> is true and the operation supports cancellation.</exception>
    /// <exception cref="TaskCanceledException">If <paramref name="forceStop"/> is true and the start of the next execution is canceled.</exception>
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