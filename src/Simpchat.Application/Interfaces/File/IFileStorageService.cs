using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.File
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(string bucketName, string objectName, Stream data, string contentType);
        Task<MemoryStream> DownloadFileAsync(string bucketName, string objectName);
        Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiryInSeconds = 604800); // 7 days default
        string GetPublicUrl(string bucketName, string objectName); // Permanent public URL
        Task<bool> FileExistsAsync(string bucketName, string objectName);
        Task<bool> RemoveFileAsync(string bucketName, string objectName);
        Task<bool> BucketExistsAsync(string bucketName);
        Task CreateBucketAsync(string bucketName);
        Task SetBucketPolicyAsync(string bucketName, string policy);
        Task MakeBucketPublicAsync(string bucketName);
    }
}
