using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMWebApp1.Helper
{
    /// <summary>
    /// Static utility class to help with file upload.
    /// </summary>
    public static class FileUploadHelper
    {
        private const double LENGTH_RATE = 1024f;
        private const double MAX_SIZE_MB = 10;
        private static readonly List<string> allowedImageExtensions = new() { ".jpg", ".png", ".jpeg", ".gif" };
        private static readonly ILogger _logger;


        /// <summary>
        /// Gets a list of supported image extensions.
        /// </summary>
        /// <returns>List of supported image extensions.</returns>
        public static List<string> AllowedImageExtensions => allowedImageExtensions;

        /// <summary>
        /// Checks whether a file exceeds the allowed size.
        /// </summary>
        /// <param name="file">The file to validate.</param>
        /// <param name="size">The max size of the file that is allowed in MB. By default it is 8 MB.</param>
        /// <returns>Whether the file size is valid or not.</returns>
        public static bool ValidFileSize(IBrowserFile file, int size = 8)
        {
            try
            {
                static double ConvertBytesToMegabytes(long bytes)
                {
                    return bytes / LENGTH_RATE / LENGTH_RATE;
                }

                return !(ConvertBytesToMegabytes(file.Size) > size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validate file extension.
        /// </summary>
        /// <param name="file">The file for which the extension will be returned.</param>
        /// <returns>Whether the file extension is within the list of allowed extension.</returns>
        public static bool ValidateImageExtension(IBrowserFile file)
        {
            string currentExtension = GetFileExtension(file);
            return AllowedImageExtensions.Contains(currentExtension);
        }

        /// <summary>
        /// Returns a validation message for unsupported image extension
        /// </summary>
        /// <returns>Error message for for unsupported image types.</returns>
        public static string ImageExtensionErrorMessage()
        {
            return $"Extension not supported. Allowed image types: {string.Join(", ", AllowedImageExtensions)}";
        }

        /// <summary>
        /// Converts file into byte array.
        /// Max file size is set to 10 MB.
        /// </summary>
        /// <param name="file">The file to convert.</param>
        /// <returns>Byte array representation of the file.</returns>
        public static async Task<byte[]> GetFileByteArray(IBrowserFile file)
        {
            try
            {
                MemoryStream ms = new();

                await file.OpenReadStream((long)(LENGTH_RATE * LENGTH_RATE * MAX_SIZE_MB)).CopyToAsync(ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the extension of a file.
        /// </summary>
        /// <param name="file">The file for which the extension will be returned.</param>
        /// <returns>The extension.</returns>
        public static string GetFileExtension(IBrowserFile file)
        {
            try
            {
                return file.ContentType.Split("/").Length == 2 ? $".{file.ContentType.Split("/")[1]}" : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public static string GetFileName(string fileName)
        {
            var data = fileName.Split('.');
            if (data.Length == 0)
            {
                return "attachment-ticket";
            }
            else
            {
                return data[0];
            }
        }
    }
}
