using Barbearia.Domain.Entities;

namespace Barbearia.Application.Interfaces;

public interface IServicoService
{
    Task<List<Servico>> ListarTodosAsync(bool apenasAtivos = false);
    Task<Servico?> ObterPorIdAsync(int id);
    Task<Servico> CriarAsync(Servico servico);
    Task<Servico> AtualizarAsync(Servico servico);
    Task AtivarDesativarAsync(int id, bool ativo);

    Task<List<Servico>> ListarPorBarbeiroAsync(int barbeiroId);
    Task AssociarServicosAsync(int barbeiroId, List<int> servicoIds);
}
