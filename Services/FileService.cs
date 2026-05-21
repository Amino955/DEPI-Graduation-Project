using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TellaStore.Services.Interfaces;
using TellaStore.Settings;

namespace TellaStore.Services;

public class FileService : IFileService
{
    private readonly Cloudinary _cloudinary;
    private readonly string[] _allowedTypes = { "image/jpeg", "image/jpg", "image/png", "image/webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FileService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("لم يتم تحديد ملف");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("حجم الملف يتجاوز 5 ميغابايت");

        if (!_allowedTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("يُسمح فقط بصور JPG و PNG و WEBP");

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Transformation = new Transformation()
                .Width(800).Height(800).Crop("limit").Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"فشل رفع الصورة: {result.Error.Message}");

        return (result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId)) return;
        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }

    public async Task<List<(string Url, string PublicId)>> UploadMultipleAsync(
        List<IFormFile> files, string folder)
    {
        var results = new List<(string Url, string PublicId)>();
        foreach (var file in files)
        {
            var result = await UploadImageAsync(file, folder);
            results.Add(result);
        }
        return results;
    }
}
