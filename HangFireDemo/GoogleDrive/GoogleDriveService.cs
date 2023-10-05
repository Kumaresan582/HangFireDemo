using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace HangFireDemo.GoogleDrive
{
    public class GoogleDriveService: IGoogleDriveService
    {
        private readonly string clientId = "148198482783-56hp0eh203akve3cfk3du0bdrp0849pj.apps.googleusercontent.com";
        private readonly string clientSecret = "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";
        private readonly string applicationName = "DriveUpload";
        private readonly string username = "kumaresanvaf135@gmail.com";

        public DriveService GetDriveService()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "ya29.a0AfB_byBU9bIhZI7Q3KA3G92qJuk9TpmVwQRBLeqqWqGe2328YIqagQFazio_bIKs5MEA0VAzA0AS7gOsM_fkt1h12UhNNsLJASMW6gDFWtWWGt4aUQOaL1fHHoW44BwTLoI8rtxRnBuxarytN-eOVc_0pE5Rf2m5pReWaCgYKAdESARMSFQGOcNnCPtRZGsp5cb79oLXm9Pavwg0171", // Replace with your access token
                RefreshToken = "1//04zeuJyVp8F3tCgYIARAAGAQSNwF-L9Irh-G8dthmp6arBsNhYh8YQzUC_V0VAaUFwVGgX2u0X3Pll-ROVwVRNyzy780OKBY0gWw" // Replace with your refresh token
            };

            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new FileDataStore(applicationName)
            });

            var credential = new UserCredential(apiCodeFlow, username, tokenResponse);

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });
        }

        public string CreateFolderInDrive(DriveService service, string folderName)
        {
            var driveFolder = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
            };

            return service.Files.Create(driveFolder).Execute().Id;
        }

        public Google.Apis.Drive.v3.Data.File FindFolderByName(DriveService service, string folderName)
        {
            var request = service.Files.List();
            request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}'";
            request.Fields = "files(id)";
            var result = request.Execute();
            return result.Files.FirstOrDefault();
        }

        public void DeleteFileFromDrive(DriveService service, string fileId)
        {
            var command = service.Files.Delete(fileId);
            command.Execute();
        }

        public Google.Apis.Drive.v3.Data.File FindFileByName(DriveService service, string fileName)
        {
            try
            {
                var request = service.Files.List();
                request.Q = $"name='{fileName}'";
                request.Fields = "files(id)";
                var result = request.Execute();

                if (result.Files != null && result.Files.Count > 0)
                {
                    return result.Files.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while searching for the file: {ex.Message}", ex);
            }
        }

        public IList<string> GetAllFileNames(DriveService service)
        {
            var fileList = service.Files.List();
            fileList.Fields = "nextPageToken, files(name)";

            var result = new List<string>();
            string pageToken = null;

            do
            {
                fileList.PageToken = pageToken;
                var filesResult = fileList.Execute();
                var files = filesResult.Files;

                foreach (var file in files)
                {
                    result.Add(file.Name);
                }

                pageToken = filesResult.NextPageToken;
            } while (pageToken != null);

            return result;
        }

        public Stream DownloadFileWithProgress(DriveService service, string fileId, Action<IDownloadProgress> progressCallback)
        {
            var request = service.Files.Get(fileId);
            var stream = new MemoryStream();

            request.MediaDownloader.ProgressChanged += progressCallback;

            request.Download(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public Stream DownloadFileFromDrive(DriveService service, string fileId)
        {
            var request = service.Files.Get(fileId);
            var stream = new MemoryStream();

            var downloadCompleted = new ManualResetEvent(false);

            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                if (progress.Status == DownloadStatus.Completed)
                {
                    downloadCompleted.Set();
                }
                else if (progress.Status == DownloadStatus.Failed)
                {
                    downloadCompleted.Set();
                }
            };

            request.Download(stream);

            downloadCompleted.WaitOne();

            // Reset the stream position to the beginning
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }

    public class CreateFolderRequest
    {
        public string FolderName { get; set; }
    }
}