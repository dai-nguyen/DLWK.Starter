using ApplicationCore.Enums;
using ApplicationCore.Extensions;
using ApplicationCore.Interfaces;
using ApplicationCore.Requests;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services
{
    public class FileService : IFileService
    {
        private static string numberPattern = " ({0})";

        private readonly ILogger _logger;

        public FileService(
            ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public async Task<string> UploadAsync(UploadRequest request)
        {
            if (request.Data == null) return string.Empty;

            try
            {
                using (var streamData = new MemoryStream(request.Data))
                {
                    if (streamData.Length == 0) return string.Empty;

                    var folder = request.UploadType.ToDescriptionString();
                    var folderName = Path.Combine("Files", folder);
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    bool exists = Directory.Exists(pathToSave);

                    if (!exists)
                        Directory.CreateDirectory(pathToSave);

                    var fileName = request.FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    if (File.Exists(dbPath))
                    {
                        dbPath = NextAvailableFilename(dbPath);
                        fullPath = NextAvailableFilename(fullPath);
                    }

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await streamData.CopyToAsync(stream);
                    }

                    return dbPath; 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to upload {0}", request);
            }

            return string.Empty;
        }

        public async Task DeleteAsync(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) return;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), dbPath);

            try
            {                
                if (!File.Exists(fullPath)) return;

                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unable to remove {0}", fullPath);
            }
            
            await Task.FromResult(0);
        }

        public static string NextAvailableFilename(string path)
        {
            // Short-cut if already available
            if (!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if (Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern);
        }

        private static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            //if (tmp == pattern)
            //throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }
    }
}
