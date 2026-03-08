namespace Barbearia.Application.Interfaces;

public interface IArquivoService
{
    Task<string> UploadAsync(Stream stream, string nomeArquivo, string containerName = "fotos");
    Task DeleteAsync(string url, string containerName = "fotos");
}
