using Microsoft.Extensions.Options;

namespace StylekAPI.Helpers;

public class FileUploadHelper
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly string _uploadRoot;
    private readonly string _baseUrl;

    public FileUploadHelper(IWebHostEnvironment env, IOptions<AppSettings> appSettings)
    {
        _uploadRoot = Path.Combine(env.ContentRootPath, appSettings.Value.UploadPath);
        _baseUrl = appSettings.Value.BaseUrl.TrimEnd('/');
    }

    public async Task<string> SaveImageAsync(IFormFile file, string subFolder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds 5MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only JPG, PNG, and WEBP files are allowed.");

        var folder = Path.Combine(_uploadRoot, subFolder);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{subFolder}/{fileName}";
    }
}
