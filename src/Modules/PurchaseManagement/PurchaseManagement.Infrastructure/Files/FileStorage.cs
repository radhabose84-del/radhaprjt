
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
using Microsoft.Extensions.Configuration;

namespace PurchaseManagement.Infrastructure.Files;

public sealed class FileStorage : IFileStorage
{
    private readonly string _root;
    public FileStorage(IConfiguration cfg)
    {
        _root = cfg["Storage:Root"] ?? Path.GetTempPath(); // e.g., "\\fileserver\reports" or a mount
    }

    public async Task<string> PutAsync(string container, string key, byte[] data, string contentType, CancellationToken ct)
    {
        var dir = Path.Combine(_root, container, Path.GetDirectoryName(key)!);
        Directory.CreateDirectory(dir);
        var full = Path.Combine(_root, container, key);
        await File.WriteAllBytesAsync(full, data, ct);

        // return a URL your email service can reach; for file share, return UNC; for web, return https URL
        // Here, return the absolute path as a pseudo "url":
        return full.Replace('\\','/'); 
    }
}
