using ClosedXML.Excel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using HelloBotNET.AppService.SqlLite.Model;
using SQLite;
//using HelloBotNET.AppService.SqlLite.Model;
//using SQLite;
using System.Data;
using System.Drawing.Imaging;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace HelloBotNET.AppService.Services
{
    /// <summary>
    /// It contains the main functionality of the telegram bot. <br />
    /// The application creates a new instance of this class to process each update received.
    /// </summary>
    public partial class HelloBot : TelegramBotBase<HelloBotProperties>
    {
        private string databasePath = Path.Combine(Directory.GetCurrentDirectory(), "MyData.db");
        private const string ApplicationName = "my-project-upload-file-363503";
        private const string AppDriveApi = "https://driveappname.herokuapp.com/api";
        public Message _Message { get; set; }
        protected override async void OnCommand(Message message, string commandName, string commandParameters)
        {
            var args = commandParameters.Split(' ');
#if DEBUG
            _logger.LogInformation("Params: {0}", args.Length);
#endif

            switch (commandName)
            {
                case "start":
                    Api.SendChatAction(message.Chat.Id, action: "typing");
                    var helloMess = string.Format("Xin chào, {0}! \n Anh chị gõ lệnh: \n /help \n Để biết cách lấy chấm công", message.From!.FirstName);
                    Api.SendMessage(message.Chat.Id, helloMess);
                    break;
                case "adminCommand":
                    Api.SendChatAction(message.Chat.Id, action: "typing");

                    if (!string.IsNullOrEmpty(commandParameters) && args.Length == 1 && commandParameters.Contains("initDB"))
                    {
                        var mess = Excel2DB();
                        Api.SendMessage(message.Chat.Id, mess);
                    }
                    break;
                case "hello": // Reply to /hello command
                    var hello = string.Format("Xin chào, {0}!", message.From!.FirstName);
                    Api.SendMessage(message.Chat.Id, hello);
                    break;
                case "help":
                    var des = "Hướng dẫn lấy ngày chấm công:\n Ví dụ a/c có mã số là NVA1 thì câu lệnh sẽ là: \n/msnv NVA1";
                    Api.SendMessage(message.Chat.Id, des);
                    break;

                case "msnv":
                    var desMsnv = "Không tìm thấy mã số nhân viên";

                    if (!string.IsNullOrEmpty(commandParameters) && args.Length == 1)
                    {
                        var clone = commandParameters;
                        var name = string.Empty;
                        using (var db = new SQLiteConnection(databasePath))
                        {
                            var str = RemoveSign4VietnameseString(clone);
                            var anyCode = db.Find<Employee>(x => x.Code.ToLower() == str.ToLower());
                            if (anyCode == null)
                            {
                                Api.SendMessage(message.Chat.Id, desMsnv);
                                break;
                            }
                            name = RemoveSign4VietnameseString(anyCode.EmployeeFullName).Replace(" ", "_");
                        }
                        Api.SendChatAction(message.Chat.Id, action: "upload_photo");


                        desMsnv = "Mã số nhân viên " + commandParameters;
                        _Message = message;

                        var streamFile = LoadFilesDrive($"{commandParameters.ToUpper()}.pdf", "Cham_Cong");

                        //var fileImage = ConvertPdfToImg(streamFile);
                        if (streamFile != null)
                            await Api.SendDocumentAsync(message.Chat.Id, new InputFile(streamFile.ToArray(), name + ".jpg"));
                        else
                        {
                            Api.SendMessage(message.Chat.Id, "Gửi thất bại");
                        }
                    }
                    else
                        Api.SendMessage(message.Chat.Id, desMsnv);
                    break;
                default:
                    if (message.Chat.Type == ChatType.Private)
                    {
                        Api.SendMessage(message.Chat.Id, "Câu lệnh không đúng.");
                    }
                    break;
            }
        }
        private string Excel2DB()
        {
            string mess = "Update Dữ liệu thất bại";
            using (SQLiteConnection db = new SQLiteConnection(databasePath))
            {
                try
                {
                    db.DeleteAll<Employee>();
                    DataTable dt = new DataTable();
                    dt = ExcelPackageToDataTable();
                    var data = ConvertDataTable<Employee>(dt);
                    var codes = data.Select(x => x.Code).ToList();
                    var anyDataCode = db.Table<Employee>().Where(s => codes.Contains(s.Code)).Select(x => x.Code);

                    if (anyDataCode.Any())
                        data.RemoveAll(x => anyDataCode.Contains(x.Code));

                    if (data.Any())
                        db.InsertAll(data);
                    mess = "Update Dữ liệu Thành công";
                    return mess;
                }
                catch (Exception ex)
                {
                    return mess;
                }
            }
        }
        private DataTable ExcelPackageToDataTable()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("Code");
            dt.Columns.Add("EmployeeFullName");
            dt.Columns.Add("EmployeeOnlyName");


            var file = Directory.GetCurrentDirectory();

            //var xlFile = Path.Combine(file, "MSNV.xlsx");
            var xlFile = LoadFilesDrive("MSNV.xlsx", "MSNV");
            using var wbook = new XLWorkbook(xlFile);

            var ws1 = wbook.Worksheet(1);

            int currentColumn = 2;
            foreach (IXLRow row in ws1.Rows().Skip(currentColumn))
            {
                if (!string.IsNullOrEmpty(row.Cell(1).Value.ToString()))
                {
                    //Add rows to DataTable.
                    dt.Rows.Add();
                    int i = 0;
                    if (row.FirstCellUsed() != null)
                    {
                        foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                        {
                            if (row.Cell(row.LastCellUsed().Address.ColumnNumber).Value == cell.Value)
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = RemoveSign4VietnameseString(cell.Value.ToString());
                            }
                            else if (row.Cell(row.FirstCellUsed().Address.ColumnNumber).Value == cell.Value)
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = RemoveSign4VietnameseString(cell.Value.ToString());
                            }
                            else
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                            }
                            i++;
                        }
                    }
                }

            }

            return dt;
        }

        private static ServiceAccountCredential GetCredentials()
        {
            //string[] scopes = new string[] { DriveService.Scope.Drive };

            //UserCredential credential;
            //using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            //{
            //    string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            //    credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

            //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //        GoogleClientSecrets.Load(stream).Secrets,
            //        scopes,
            //        "user",
            //        //"doannhatminhhaicv@gmail.com",
            //        CancellationToken.None,
            //        new FileDataStore(credPath, true)).Result;
            //}
            //return credential;
            string[] scopes = new string[] { DriveService.Scope.Drive }; // Full access

            var keyFilePath = @"client_P12.p12";    // Downloaded from https://console.developers.google.com
            var serviceAccountEmail = "data-test@my-project-upload-file-363503.iam.gserviceaccount.com";  // found https://console.developers.google.com

            //loading the Key file
            var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = scopes,

            }.FromCertificate(certificate));
            return credential;

        }

        private MemoryStream LoadFilesDrive(string fileName, string folderName)
        {
            try
            {
                //Google.Apis.Services.BaseClientService.Initializer bcs = new Google.Apis.Services.BaseClientService.Initializer
                //{
                //    ApiKey = "AIzaSyBWuQc-2eiRYHAN6cjuOFKXDmeXxA2OwCo",
                //    ApplicationName = ApplicationName,
                //};

                //Google.Apis.Drive.v3.DriveService service = new Google.Apis.Drive.v3.DriveService(bcs);

                // Create Drive API service.

                //var foderName = "Cham_Cong";
                var folderId = GetFolder(folderName);

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = GetCredentials(),
                    ApplicationName = ApplicationName,

                });
                var request = service.Files.List();
                request.PageSize = 100;
                request.Fields = "nextPageToken, files(id,name,parents)";

                request.Q = $"'{folderId}' in parents and name = '{fileName}'"; // not in brackets!


                var result = request.Execute();
                var fileId = result.Files.Any() ? result.Files.FirstOrDefault()?.Id : String.Empty;
                if (!string.IsNullOrEmpty(fileId))
                {
                    var requestDownload = service.Files.Get(fileId);
                    var stream = new MemoryStream();
                    //Add a handler which will be notified on progress changes.
                    // It will notify on each chunk download and when the
                    // download is completed or failed.
                    requestDownload.MediaDownloader.ProgressChanged +=
                        progress =>
                        {
                            switch (progress.Status)
                            {
                                case DownloadStatus.Downloading:
                                    {
                                        Console.WriteLine(progress.BytesDownloaded);
                                        break;
                                    }
                                case DownloadStatus.Completed:
                                    {
                                        Console.WriteLine("Download complete.");
                                        break;
                                    }
                                case DownloadStatus.Failed:
                                    {
                                        Console.WriteLine("Download failed.");
                                        break;
                                    }
                            }
                        };
                    requestDownload.Download(stream);

                    return stream;
                }
                return null;
            }
            catch (Exception e)
            {

                Api.SendMessage(_Message.Chat.Id, "google file :" + e.Message);
                // TODO(developer) - handle error appropriately
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else
                {
                    Api.SendMessage(_Message.Chat.Id, "exception:" + e.Message);

                    //throw;
                }
            }
            return null;
        }

        private string GetFolder(string name)
        {
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = GetCredentials(),
                    ApplicationName = ApplicationName

                });
                var request = service.Files.List();

                request.PageSize = 1000;
                request.Fields = "nextPageToken, files(id,name,parents)";
                request.Q = $"name = '{name}' and mimeType = 'application/vnd.google-apps.folder'"; // not in brackets!

                var result = request.Execute();
                var fileId = result.Files.Any() ? result.Files.FirstOrDefault()?.Id : String.Empty;




                return fileId;
            }
            catch (Exception e)
            {
                Api.SendMessage(_Message.Chat.Id, "google folder :" + e.Message);
                // TODO(developer) - handle error appropriately
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else
                {
                    throw;
                }
            }
            return string.Empty;
        }

        private byte[] ConvertPdfToImg(MemoryStream stream)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    
#pragma warning disable CA1416 // Validate platform compatibility
                    PDFtoImage.Conversion.SaveJpeg(ms, stream.ToArray());
#pragma warning restore CA1416 // Validate platform compatibility
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                //Api.SendMessage(Api.mes.Chat.Id, mess);
                return null;
            }

        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        private static readonly string[] VietnameseSigns = new string[]
        {

            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"
        };
        public static string RemoveSign4VietnameseString(string str = "")
        {
            if (string.IsNullOrEmpty(str))
                return str;

            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }
    }
}
