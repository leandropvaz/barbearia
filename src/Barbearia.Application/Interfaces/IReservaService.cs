using Barbearia.Domain.Entities;
using Barbearia.Domain.Enums;

namespace Barbearia.Application.Interfaces;

public interface IReservaService
{
    Task<List<Reserva>> ListarPorBarbeiroAsync(int barbeiroId, DateTime? data = null);
    Task<List<Reserva>> ListarTodasAsync(DateTime? data = null, StatusReserva? status = null);
    Task<Reserva?> ObterPorIdAsync(int id);
    Task<Reserva?> ObterPorCodigoAsync(string codigo);
    Task<Reserva> CriarAsync(Reserva reserva);
    Task<Reserva> AtualizarAsync(Reserva reserva);
    Task ConfirmarAsync(int id);
    Task CancelarAsync(int id, string motivo);
    Task ConcluirAsync(int id);
    Task<List<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, int servicoId, DateTime data);
}
