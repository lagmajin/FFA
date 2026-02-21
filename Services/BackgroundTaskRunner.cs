using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FFA.Services;

// Lightweight background task runner that captures exceptions and supports cancellation.
// Use Run(...) to start a background operation; it returns a CancellationTokenSource
// that can be cancelled by the caller.
public class BackgroundTaskRunner
{
    private readonly ILogger<BackgroundTaskRunner> _logger;

    public BackgroundTaskRunner(ILogger<BackgroundTaskRunner> logger)
    {
        _logger = logger;
    }

    // Start a background task. The returned CancellationTokenSource can be used to cancel it.
    public CancellationTokenSource Run(Func<CancellationToken, Task> work)
    {
        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            try
            {
                await work(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                _logger.LogInformation("Background task cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background task threw an exception");
            }
        }, CancellationToken.None);

        return cts;
    }
}
