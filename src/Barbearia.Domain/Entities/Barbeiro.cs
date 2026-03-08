namespace Barbearia.Domain.Entities;

public class Barbeiro
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public bool Ativo { get; set; } = true;
    public string SenhaHash { get; set; } = string.Empty;
    public bool AgendaAberta { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<BarbeiroServico> BarbeiroServicos { get; set; } = new List<BarbeiroServico>();
    public ICollection<HorarioBarbeiro> Horarios { get; set; } = new List<HorarioBarbeiro>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<BloqueioAgenda> Bloqueios { get; set; } = new List<BloqueioAgenda>();
}
