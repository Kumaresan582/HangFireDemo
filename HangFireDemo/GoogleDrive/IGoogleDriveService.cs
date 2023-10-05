using Google.Apis.Download;
using Google.Apis.Drive.v3;

namespace HangFireDemo.GoogleDrive
{
    public interface IGoogleDriveService
    {
        public DriveService GetDriveService();
        public string CreateFolderInDrive(DriveService service, string folderName);
        public Google.Apis.Drive.v3.Data.File FindFolderByName(DriveService service, string folderName);
        public void DeleteFileFromDrive(DriveService service, string fileId);
        public Google.Apis.Drive.v3.Data.File FindFileByName(DriveService service, string fileName);
        public IList<string> GetAllFileNames(DriveService service);
        public Stream DownloadFileWithProgress(DriveService service, string fileId, Action<IDownloadProgress> progressCallback);
        public Stream DownloadFileFromDrive(DriveService service, string fileId);
    }
}
