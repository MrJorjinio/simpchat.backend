using Microsoft.AspNetCore.Http;
using Simpchat.Application.Errors;
using Simpchat.Application.Models.ApiResult;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Validators
{
    public static class FileValidator
    {
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB in bytes

        private static readonly string[] AllowedImageTypes =
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        private static readonly string[] AllowedFileTypes =
        {
            // Images
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp",
            // Documents
            "application/pdf",
            "text/plain",
            // Microsoft Office
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" // .pptx
        };

        public static Result ValidateFile(IFormFile file, bool imageOnly = false)
        {
            if (file == null || file.Length == 0)
            {
                return Result.Success(); // No file to validate
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                return Result.Failure(ApplicationErrors.File.TooLarge);
            }

            // Check file type
            var allowedTypes = imageOnly ? AllowedImageTypes : AllowedFileTypes;
            var contentType = file.ContentType.ToLowerInvariant();

            if (!allowedTypes.Contains(contentType))
            {
                return Result.Failure(ApplicationErrors.File.InvalidType);
            }

            return Result.Success();
        }
    }
}
