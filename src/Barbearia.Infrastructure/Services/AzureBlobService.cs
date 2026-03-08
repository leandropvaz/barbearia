using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Barbearia.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Barbearia.Infrastructure.Services;

public class AzureBlobService : IArquivoService
{
    private readonly string _connectionString;
    private readonly string _baseUrl;

    public AzureBlobService(IConfiguration config)
    {
        _connectionString = config["Azure:StorageConnectionString"]
            ?? throw new InvalidOperationException("Azure:StorageConnectionString não configurado.");
        _baseUrl = config["Azure:StorageBaseUrl"] ?? string.Empty;
    }

    public async Task<string> UploadAsync(Stream stream, string nomeArquivo, string containerName = "fotos")
    {
        var client = new BlobServiceClient(_connectionString);
        var container = client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blob = container.GetBlobClient(nomeArquivo);
        var extensao = Path.GetExtension(nomeArquivo).ToLower();
        var contentType = extensao switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string url, string containerName = "fotos")
    {
        if (string.IsNullOrEmpty(url)) return;
        var nomeArquivo = Path.GetFileName(new Uri(url).LocalPath);
        var client = new BlobServiceClient(_connectionString);
        var container = client.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(nomeArquivo);
        await blob.DeleteIfExistsAsync();
    }
}
