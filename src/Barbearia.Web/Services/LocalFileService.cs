using Barbearia.Application.Interfaces;

namespace Barbearia.Web.Services;

public class LocalFileService : IArquivoService
{
    private readonly string _webRootPath;

    public LocalFileService(IWebHostEnvironment env)
    {
        _webRootPath = env.WebRootPath;
    }

    public async Task<string> UploadAsync(Stream stream, string nomeArquivo, string containerName = "fotos")
    {
        var pasta = Path.Combine(_webRootPath, "images", "barbeiros");
        Directory.CreateDirectory(pasta);

        var caminhoFisico = Path.Combine(pasta, nomeArquivo);
        using var fs = File.Create(caminhoFisico);
        await stream.CopyToAsync(fs);

        return $"/images/barbeiros/{nomeArquivo}";
    }

    public Task DeleteAsync(string url, string containerName = "fotos")
    {
        if (string.IsNullOrEmpty(url)) return Task.CompletedTask;

        var nomeArquivo = Path.GetFileName(url);
        var caminhoFisico = Path.Combine(_webRootPath, "images", "barbeiros", nomeArquivo);
        if (File.Exists(caminhoFisico)) File.Delete(caminhoFisico);

        return Task.CompletedTask;
    }
}
