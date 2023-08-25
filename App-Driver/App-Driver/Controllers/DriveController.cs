using ClosedXML.Excel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace App_Driver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriveController : ControllerBase
    {
        private const string ApplicationName = "my-project-upload-file-363503";
        public DriveController()
        {

        }
        [HttpGet]
        public async Task<IActionResult> LoadFile(string fileName, string name)
        {
            try
            {
                string inputStr = Encoding.UTF8.GetString(Convert.FromBase64String(fileName));
                string nameDownload = Encoding.UTF8.GetString(Convert.FromBase64String(name));
                var folderName = "Cham_Cong";
                var file = await LoadFilesDrive(inputStr, folderName);
                if(file != null)
                {
                    using (var ms = new MemoryStream())
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        PDFtoImage.Conversion.SaveJpeg(ms, file.ToArray());
#pragma warning restore CA1416 // Validate platform compatibility
                        return File(ms.ToArray(), "image/jpeg", nameDownload + ".jpg");
                    }
                }
                return Ok("Not Found");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
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
            var fileExcel = LoadFilesDrive("MSNV.xlsx", "MSNV").GetAwaiter().GetResult();
            using var wbook = new XLWorkbook(fileExcel);

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

        private async Task<MemoryStream> LoadFilesDrive(string fileName, string folderName)
        {
            var folderId = await GetFolder(folderName);


            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = GetCredentials(),
                ApplicationName = ApplicationName,

            });
            var request = service.Files.List();
            request.PageSize = 100;
            request.Fields = "nextPageToken, files(id,name,parents)";

            request.Q = $"'{folderId}' in parents and name = '{fileName.ToUpper()}.pdf'"; // not in brackets!


            var result = await request.ExecuteAsync();
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

        private async Task<string> GetFolder(string name)
        {
            // Create Drive API service.
            //Google.Apis.Services.BaseClientService.Initializer bcs = new Google.Apis.Services.BaseClientService.Initializer();
            //bcs.ApiKey = "AIzaSyBWuQc-2eiRYHAN6cjuOFKXDmeXxA2OwCo";
            //bcs.ApplicationName = ApplicationName;

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = GetCredentials(),
                ApplicationName = ApplicationName,
                ApiKey = "AIzaSyBWuQc-2eiRYHAN6cjuOFKXDmeXxA2OwCo"

            });
            var request = service.Files.List();

            request.PageSize = 1000;
            request.Fields = "nextPageToken, files(id,name,parents)";
            request.Q = $"name = '{name}' and mimeType = 'application/vnd.google-apps.folder'"; // not in brackets!

            var result = await request.ExecuteAsync();
            var fileId = result.Files.Any() ? result.Files.FirstOrDefault()?.Id : String.Empty;


            return fileId;
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
