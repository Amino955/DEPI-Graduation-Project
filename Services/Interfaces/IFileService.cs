using Microsoft.AspNetCore.Http;

namespace TellaStore.Services.Interfaces;

public interface IFileService
{
    Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folder);
    Task DeleteImageAsync(string publicId);
    Task<List<(string Url, string PublicId)>> UploadMultipleAsync(List<IFormFile> files, string folder);
}
