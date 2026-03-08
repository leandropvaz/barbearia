using Barbearia.Domain.Enums;

namespace Barbearia.Domain.Entities;

public class Reserva
{
    public int Id { get; set; }
    public int BarbeiroId { get; set; }
    public Barbeiro Barbeiro { get; set; } = null!;
    public int ServicoId { get; set; }
    public Servico Servico { get; set; } = null!;

    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;

    public DateTime DataHora { get; set; }
    public StatusReserva Status { get; set; } = StatusReserva.Confirmada;
    public string CodigoConfirmacao { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    public string? MotivoCancelamento { get; set; }
}
