using Barbearia.Domain.Entities;

namespace Barbearia.Application.Interfaces;

public interface IBarbeiroService
{
    Task<List<Barbeiro>> ListarTodosAsync(bool apenasAtivos = false);
    Task<Barbeiro?> ObterPorIdAsync(int id);
    Task<Barbeiro?> ObterPorEmailAsync(string email);
    Task<Barbeiro> CriarAsync(Barbeiro barbeiro, string senha);
    Task<Barbeiro> AtualizarAsync(Barbeiro barbeiro, string? novaSenha = null);
    Task AtivarDesativarAsync(int id, bool ativo);
    Task AbrirFecharAgendaAsync(int id, bool aberta);
    Task<bool> ValidarSenhaAsync(string email, string senha);
    Task<string?> AtualizarFotoAsync(int id, Stream fotoStream, string nomeArquivo);
    Task ExcluirFotoAsync(int id);
    Task SalvarHorariosAsync(int barbeiroId, List<HorarioBarbeiro> horarios);
    Task<List<BloqueioAgenda>> ListarBloqueiosAsync(int barbeiroId, DateTime data);
    Task BloquearSlotAsync(int barbeiroId, DateTime inicio, DateTime fim, string? motivo = null);
    Task DesbloquearSlotAsync(int id);
}
