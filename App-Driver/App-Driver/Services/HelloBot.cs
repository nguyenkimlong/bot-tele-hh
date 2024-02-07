using Microsoft.Extensions.Caching.Distributed;
using Telegram.BotAPI;

namespace HelloBotNET.AppService.Services
{
    /// <summary>
    /// It contains the main functionality of the telegram bot. <br />
    /// The application creates a new instance of this class to process each update received.
    /// </summary>
    public partial class HelloBot : TelegramBotBase<HelloBotProperties>
    {
        private readonly ILogger<HelloBot> _logger;

        private readonly IDistributedCache _memoryCache;
        public HelloBot(ILogger<HelloBot> logger, IDistributedCache memoryCache, HelloBotProperties botProperties) : base(botProperties)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }
    }
}
