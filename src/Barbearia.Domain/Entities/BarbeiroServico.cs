namespace Barbearia.Domain.Entities;

public class BarbeiroServico
{
    public int Id { get; set; }
    public int BarbeiroId { get; set; }
    public Barbeiro Barbeiro { get; set; } = null!;
    public int ServicoId { get; set; }
    public Servico Servico { get; set; } = null!;
}
