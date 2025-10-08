using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ozge.Core.Contracts;

namespace Ozge.App.Infrastructure;

public sealed class BackgroundJobHostService : BackgroundService
{
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<BackgroundJobHostService> _logger;

    public BackgroundJobHostService(IJobQueue jobQueue, ILogger<BackgroundJobHostService> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _jobQueue.DequeueAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            _logger.LogDebug("No-op background job {JobId}", job.Id);
            await _jobQueue.MarkCompletedAsync(job.Id, stoppingToken);
        }
    }
}
