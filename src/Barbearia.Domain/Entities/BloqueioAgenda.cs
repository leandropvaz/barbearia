namespace Barbearia.Domain.Entities;

public class BloqueioAgenda
{
    public int Id { get; set; }
    public int BarbeiroId { get; set; }
    public Barbeiro Barbeiro { get; set; } = null!;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string? Motivo { get; set; }
}
