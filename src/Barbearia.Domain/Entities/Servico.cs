namespace Barbearia.Domain.Entities;

public class Servico
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int DuracaoMinutos { get; set; } = 30;
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<BarbeiroServico> BarbeiroServicos { get; set; } = new List<BarbeiroServico>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
