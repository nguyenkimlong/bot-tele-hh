using HelloBotNET.AppService;
using HelloBotNET.AppService.Services;
using Serilog;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;

namespace App_Driver.Worker
{
    public class Worker : BackgroundService
    {
        private readonly BotClient _api;
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan sleepInterval = TimeSpan.FromSeconds(60); // Sleep for 60 seconds

        private const string LogFolderPath = "logs"; // Specify the folder path for your logs
        private const int CleanupIntervalInDays = 7;


        public Worker(ILogger<Worker> logger, HelloBotProperties botProperties, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _api = botProperties.Api;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            // Long Polling
            var updates = await _api.GetUpdatesAsync(cancellationToken: stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (updates.Any())
                    {
                        
                        Parallel.ForEach(updates, (update) => ProcessUpdate(update));

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
                }
                CleanupOldLogs();
            }

        }

        private void ProcessUpdate(Update update)
        {
            _logger.LogInformation("User Action: {Time}, message: {text}", update.Message.From?.Id, update.Message.Text);
            using var scope = _serviceProvider.CreateScope();
            var bot = scope.ServiceProvider.GetRequiredService<HelloBot>();
            bot.OnUpdate(update);
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
