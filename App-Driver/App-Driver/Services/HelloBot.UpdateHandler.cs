using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;

namespace HelloBotNET.AppService.Services
{
    /// <summary>
    /// It contains the main functionality of the telegram bot. <br />
    /// The application creates a new instance of this class to process each update received.
    /// </summary>
    public partial class HelloBot : TelegramBotBase<HelloBotProperties>
    {
        public override void OnUpdate(Update update)
        {
#if DEBUG
            _logger.LogInformation("New update with id: {0}. Type: {1}", update?.UpdateId, update?.Type.ToString("F"));
#endif

            if (update != null)
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    var query = update.CallbackQuery;
                    //Api.EditMessageText(new EditMessageTextArgs
                    //{
                    //    ChatId = query.Message.Chat.Id,
                    //    MessageId = query.Message.MessageId,
                    //    Text = $"{query.Data}"
                    //});

                    //var mess = new EditMessageMediaArgs
                    //{
                    //    ChatId = query.Message.Chat.Id,
                    //    MessageId = query.Message.MessageId,
                    //};

                    //mess.Media = new InputMediaDocument
                    //{
                    //    Caption = query.Message.Caption,
                    //    Media = "multipart/form-data"
                    //};
                    //var streamFile = LoadFilesDrive($"{query.Data}.pdf".ToUpper());
                    //mess.AttachedFiles.Add(new Telegram.BotAPI.AvailableTypes.AttachedFile(query.Data, new InputFile(streamFile.ToArray(), query.Message.From!.FirstName + ".pdf")));



                }

                //if (update.Type == UpdateType.Message)
                //{
                //    var message = update.Message;
                //    Api.SendChatAction(message.Chat.Id, ChatAction.Typing);
                //    if (message.Document != null)
                //    {
                //        try
                //        {
                //            //string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                //            string credPath = Directory.GetCurrentDirectory();

                //            var folder = "SalaryFile";
                //            credPath = Path.Combine(credPath, folder);

                //            var file = message.Document.FileName;
                //            var pathFile = Path.Combine(credPath, file);

                //            if (!Directory.Exists(credPath))
                //            {
                //                Directory.CreateDirectory(credPath);
                //            }
                //            var dataFile = Api.GetFile(message.Document.FileId);
                //            var downloadURL = $"https://api.telegram.org/file/bot{Api.Token}/{dataFile.FilePath}";
                //            using (var http = new HttpClient())
                //            {
                //                var a = http.GetAsync(downloadURL).GetAwaiter().GetResult();

                //                var dataStream = a.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

                //                if (Directory.Exists(pathFile))
                //                {
                //                    Directory.Delete(pathFile);
                //                }

                //                using (FileStream fs = System.IO.File.Create(pathFile))
                //                {

                //                    using (var memory = new MemoryStream())
                //                    {
                //                        dataStream.CopyTo(memory);
                //                        byte[] author = memory.GetBuffer();
                //                        fs.Write(author, 0, author.Length);
                //                        // Test and work with the stream here. 
                //                        // If you need to start back at the beginning, be sure to Seek again.

                //                    }

                //                }

                //            }
                //        }
                //        catch (Exception ex)
                //        {

                //            Api.SendMessage(message.Chat.Id, ex.Message);

                //        }





                //    }
                //    Api.SendMessage(message.Chat.Id, "Hello World 123!");
                //}

            }



            base.OnUpdate(update);
        }


    }
}
