using DocumentFormat.OpenXml.Spreadsheet;
using HelloBotNET.AppService;
using HelloBotNET.AppService.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace App_Driver.Worker
{
    public class Worker : BackgroundService
    {
        private readonly BotClient _api;
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HelloBot botService;
        private readonly TimeSpan sleepInterval = TimeSpan.FromSeconds(60); // Sleep for 60 seconds
        private volatile bool _isFinished = false;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        private const string LogFolderPath = "logs"; // Specify the folder path for your logs
        private const int CleanupIntervalInDays = 7;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, HelloBot bot)
        {
            _logger = logger;
            _api = bot.Api;
            _serviceProvider = serviceProvider;
            botService = bot;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            // Long Polling
            var updates = await _api.GetUpdatesAsync(cancellationToken: stoppingToken);

            _isFinished = false;
            // DoSomeWork           

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (updates.Any())
                    {
                        Parallel.ForEach(updates.Distinct(), (update) => ProcessUpdate(update));
                        updates = await _api.GetUpdatesAsync(updates[^1].UpdateId + 1, cancellationToken: stoppingToken);
                    }
                    else
                    {
                        updates = await _api.GetUpdatesAsync(cancellationToken: stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during ExecuteAsync");
                    _isFinished = true;
                    await WaitToRestartTask(stoppingToken);
                    break;                   
                }
                CleanupOldLogs();
            }
        }

        private async Task WaitToRestartTask(CancellationToken stoppingToken)
        {
            // wait until _semaphore.Release()
            await _semaphore.WaitAsync(stoppingToken);
            // run again
            await base.StartAsync(stoppingToken);
        }
        public void RestartTask()
        {
            if (!_isFinished)
                throw new InvalidOperationException("Background service is still working");

            // enter from _semaphore.WaitAsync
            _semaphore.Release();
        }
        private void ProcessUpdate(Update update)
        {
            _logger.LogInformation("User Action: {Time}, message: {text}", update.Message.From?.Id, update.Message.Text);

            if (botService != null)
            {
                botService.OnUpdate(update);
            }
            else
            {
                using var scope = _serviceProvider.CreateScope();
                var bot = scope.ServiceProvider.GetRequiredService<HelloBot>();
                bot.OnUpdate(update);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stopping at: {Time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }


        private void CleanupOldLogs()
        {
            try
            {
                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolderPath);

                if (Directory.Exists(logDirectory))
                {
                    var cutoffDate = DateTime.UtcNow.AddDays(-CleanupIntervalInDays);

                    var logFiles = Directory.GetFiles(logDirectory, "*.txt");

                    foreach (var logFile in logFiles)
                    {
                        var fileInfo = new FileInfo(logFile);

                        if (fileInfo.LastWriteTimeUtc < cutoffDate)
                        {
                            fileInfo.Delete();
                            Log.Information("Deleted log file: {file}", logFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during log cleanup");
            }
        }
    }
}
