using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using poc_amax_attachments.Data;
using System;

namespace poc_amax_attachments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3Controller : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName = "bucket-for-amax-poc";

        public S3Controller(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles(string clientId, string agentId, string databaseName)
        {
            try
            {
                // Check if the database folder exists, create it if it doesn't
               await EnsureDatabaseFolderExistsAsync(databaseName);

                // List files in the specified database folder
                var listObjectsRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = $"{databaseName}/"
                };

                var response = await _s3Client.ListObjectsV2Async(listObjectsRequest);

                var fileUrls = new List<string>();
                foreach (var item in response.S3Objects)
                {
                    var fileUrl = GetFileUrl(item.Key);
                    fileUrls.Add(fileUrl);
                }

                return Ok(fileUrls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error listing files: {ex.Message}");
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                AppDbContext dbContext = new AppDbContext();

                var sup = dbContext.tblSupplementals.Where(s => s.Id == id).FirstOrDefault();

                if (sup != null)
                {
                    sup.Deleted = true;
                    sup.DeletedBy = 1;
                    sup.DateModified = DateTime.UtcNow;
                    sup.SysUtcDateModified = DateTime.UtcNow;
                    dbContext.SaveChanges();
                }
                return Ok(sup);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var file = Request.Form.Files[0]; // Assuming only one file is uploaded
                string clientId = Request.Form["clientId"].ToString();
                string agentId = Request.Form["agentId"].ToString();
                string databaseName = Request.Form["databaseName"].ToString();

                int SupId = AddDocumentInDb(clientId, agentId);

                // Ensure the database folder exists before uploading the file
                await EnsureDatabaseFolderExistsAsync(databaseName);

                var key = $"{databaseName}/{Guid.NewGuid().ToString()}-{file.FileName}";

                using (var stream = file.OpenReadStream())
                {
                    await UploadToS3Async(stream, key);
                }

                var fileUrl = GetFileUrl(key);

                //Update Status throwing error: Data cannot be null
                UpdateDocumentStatus(SupId, fileUrl, file.FileName, true);

                return Ok(new { Url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        private async Task UploadToS3Async(Stream stream, string key)
        {
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    BucketName = _bucketName,
                    Key = key,
                    //CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private async Task EnsureDatabaseFolderExistsAsync(string databaseName)
        {
            var folderKey = $"{databaseName}/";
            var listObjectsRequest = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = folderKey,
                MaxKeys = 1
            };

            var response = await _s3Client.ListObjectsV2Async(listObjectsRequest);
            if (response.S3Objects.Count == 0)
            {
                // Folder doesn't exist, create it
                await _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = folderKey,
                    InputStream = new MemoryStream() // Use empty stream to create an empty object representing the folder
                });
            }
        }

        private string GetFileUrl(string key)
        {
            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }

        private int AddDocumentInDb(string clientId, string agentId)
        {
            AppDbContext dbContext = new AppDbContext();

            tblSupplemental sup = new tblSupplemental();

            sup.OwnerId = int.Parse(clientId);
            sup.AgentId = int.Parse(agentId);
            sup.UploadCompleted = false;
            sup.Deleted = false;
            sup.DateCreated = DateTime.UtcNow;
            sup.DateModified = DateTime.UtcNow;
            sup.SysUtcDateCreated = DateTime.UtcNow;
            sup.SysUtcDateModified = DateTime.UtcNow;

            // To avoid null exception
            sup.Description = "";
            sup.EmailUidl = "";
            sup.EmailFolder = 0;
            sup.NewFilename = string.Empty;
            sup.ObjectName = string.Empty;
            sup.FileName = string.Empty;
            sup.FinishedRename = false;
            sup.IpAddress = string.Empty;
            sup.Md5 = string.Empty;
            sup.ObjectName = string.Empty;
            sup.ObjectType = 0;
            sup.OldFilename = string.Empty;
            sup.OwnerType = 0;
            sup.Workstation = string.Empty;

            dbContext.Add(sup);

            dbContext.SaveChanges();

            return sup.Id;

        }

        private void UpdateDocumentStatus(int SupId, string FileUrl, string FileName, bool UploadCompleted)
        {
            AppDbContext dbContext = new AppDbContext();

            var sup = dbContext.tblSupplementals.Where(s => s.Id == SupId).FirstOrDefault();

            if (sup != null)
            {
                sup.UploadCompleted = UploadCompleted;
                sup.FileName = FileName;
                sup.ObjectName = FileUrl;
                sup.NewFilename = FileName;
                sup.OldFilename = FileName;
                dbContext.SaveChanges();
            }
        }

    }
}
