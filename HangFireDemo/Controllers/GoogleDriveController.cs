using HangFireDemo.DbBackUp;
using HangFireDemo.GoogleDrive;
using Microsoft.AspNetCore.Mvc;

namespace HangFireDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleDriveController : ControllerBase
    {
        private readonly IGoogleDriveService _googleDriveService;
        private readonly IDataBaseBackUp _dataBaseBackUp;

        public GoogleDriveController(IGoogleDriveService googleDriveService, IDataBaseBackUp dataBaseBackUp)
        {
            _googleDriveService = googleDriveService;
            _dataBaseBackUp = dataBaseBackUp;
        }

        [HttpPost("CreateFolder")]
        public ActionResult<string> CreateFolder([FromForm] CreateFolderRequest request)
        {
            try
            {
                var service = _googleDriveService.GetDriveService();

                // Check if the folder already exists
                var existingFolder = _googleDriveService.FindFolderByName(service, request.FolderName);

                if (existingFolder != null)
                {
                    return Ok($"Folder already exists with ID: {existingFolder.Id}");
                }

                // Folder doesn't exist, create a new one
                var folderId = _googleDriveService.CreateFolderInDrive(service, request.FolderName);
                return Ok($"Created folder with ID: {folderId}");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("UploadFile")]
        public async Task<ActionResult<string>> UploadFile(string folderName)
        {
            try
            {
                string localFilePath = @"C:\Users\visualapp\Downloads\output.txt";
                var service = _googleDriveService.GetDriveService();

                if (!System.IO.File.Exists(localFilePath))
                {
                    return BadRequest("Local file does not exist.");
                }

                using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    var folder = _googleDriveService.FindFolderByName(service, folderName);

                    if (folder == null)
                    {
                        throw new Exception($"Folder '{folderName}' not found in Google Drive.");
                    }

                    string fileName = Path.GetFileName(((FileStream)fileStream).Name);

                    var fileMetadata = new Google.Apis.Drive.v3.Data.File
                    {
                        Name = fileName,
                        Parents = new List<string> { folder.Id }
                    };

                    var request = service.Files.Create(fileMetadata, fileStream, "*/*");
                    request.Upload();

                    return Ok($"Uploaded file with ID: {fileName}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("DeleteFile/{fileName}")]
        public ActionResult DeleteFile(string fileName)
        {
            try
            {
                var service = _googleDriveService.GetDriveService();
                var file = _googleDriveService.FindFileByName(service, fileName);

                if (file != null)
                {
                    _googleDriveService.DeleteFileFromDrive(service, file.Id);
                    return Ok($"Deleted file with name: {fileName}");
                }
                else
                {
                    return NotFound($"File with name '{fileName}' not found in Google Drive.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetFiles")]
        public ActionResult<IList<string>> GetFiles()
        {
            try
            {
                var service = _googleDriveService.GetDriveService();
                var files = _googleDriveService.GetAllFileNames(service);
                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("DownloadFile")]
        public IActionResult DownloadFile(string fileName)
        {
            try
            {
                var service = _googleDriveService.GetDriveService();
                var file = _googleDriveService.FindFileByName(service, fileName);

                if (file != null)
                {
                    // Download the file content
                    var fileStream = _googleDriveService.DownloadFileFromDrive(service, file.Id);

                    return File(fileStream, "application/octet-stream", fileName);
                }
                else
                {
                    return NotFound($"File with name '{fileName}' not found in Google Drive.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("Back")]
        public IActionResult back()
        {
            _dataBaseBackUp.TriggerBackup();
            return Ok();
        }
    }
}