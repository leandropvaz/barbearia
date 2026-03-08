using Barbearia.Domain.Entities;

namespace Barbearia.Application.Interfaces;

public interface INotificacaoService
{
    Task EnviarConfirmacaoEmailAsync(Reserva reserva);
    Task EnviarConfirmacaoWhatsAppAsync(Reserva reserva);
    Task EnviarCancelamentoEmailAsync(Reserva reserva);
    Task EnviarCancelamentoWhatsAppAsync(Reserva reserva);
    Task EnviarAlteracaoEmailAsync(Reserva reserva);
    Task EnviarLembreteAsync(Reserva reserva);
}
