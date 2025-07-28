using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Services
{
    public class CrawlingLoopingService(CrawlingService crawlingService) : LoopingService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await crawlingService.Execute(new Protocols.Request.Feed { All = true });
                }
                catch (Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
