using System.Text;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace HelloBotNET.AppService
{
    /// <summary>
    /// This class defines all the necessary settings and properties for the bot to work. <br />
    /// The application uses a single instance of this class.
    /// </summary>
    public sealed class HelloBotProperties : IBotProperties
    {
        private readonly BotCommandHelper _commandHelper;

        public HelloBotProperties(IConfiguration configuration)
        {
            var telegram = configuration.GetSection("Telegram"); // JSON: "Telegram"
            var botToken = telegram["BotToken"]; // ENV: Telegram__BotToken, JSON: "Telegram:BotToken"

            Api = new BotClient(botToken);
            User = Api.GetMe();

            _commandHelper = new BotCommandHelper(this);

            var updates = Api.GetUpdates();
            // Delete my old commands
            Api.DeleteMyCommands();
            // Set my commands
            Api.SetMyCommands(
                new BotCommand("start", "Bắt đầu"),
                new BotCommand("help", "Hướng dẫn lấy ngày chấm công")
                );

            // Delete webhook to use Long Polling
            Api.DeleteWebhook();
            //while (true)
            //{
            //    if (updates.Length > 0)
            //    {
            //        foreach (var update in updates)
            //        {
            //            if (update.Type == UpdateType.Message)
            //            {
            //                var message = update.Message;
            //                Api.SendChatAction(message.Chat.Id, ChatAction.Typing);
            //                if (message.Document != null)
            //                {
            //                    string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            //                    var folder = "SalaryFile";
            //                    credPath = Path.Combine(credPath, folder);

            //                    var file = message.Document.FileName;
            //                    var pathFile = Path.Combine(credPath, file);

            //                    if (!Directory.Exists(credPath))
            //                    {
            //                        Directory.CreateDirectory(credPath);
            //                    }
            //                    var dataFile = Api.GetFile(message.Document.FileId);
            //                    var downloadURL = $"https://api.telegram.org/file/bot{botToken}/{dataFile.FilePath}";
            //                    using (var http = new HttpClient())
            //                    {
            //                        var a = http.GetAsync(downloadURL).GetAwaiter().GetResult();

            //                        var dataStream = a.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            //                        if (Directory.Exists(pathFile))
            //                        {
            //                            Directory.Delete(pathFile);
            //                        }

            //                        using (FileStream fs = System.IO.File.Create(pathFile))
            //                        {
                                       
            //                            using (var memory = new MemoryStream())
            //                            {
            //                                dataStream.CopyTo(memory);
            //                                byte[] author = memory.GetBuffer();
            //                                fs.Write(author, 0, author.Length);
            //                                // Test and work with the stream here. 
            //                                // If you need to start back at the beginning, be sure to Seek again.

            //                            }

            //                        }

            //                    }




            //                }
            //                Api.SendMessage(message.Chat.Id, "Hello World 123!");
            //            }
            //        }
            //        updates = Api.GetUpdates(offset: updates.Max(u => u.UpdateId) + 1);
            //    }
            //    else
            //    {
            //        updates = Api.GetUpdates();
            //    }
            //}

           
        }

        public BotClient Api { get; }
        public User User { get; }

        IBotCommandHelper IBotProperties.CommandHelper => _commandHelper;
    }
}
