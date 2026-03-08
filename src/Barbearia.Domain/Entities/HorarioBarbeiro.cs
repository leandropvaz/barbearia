namespace Barbearia.Domain.Entities;

public class HorarioBarbeiro
{
    public int Id { get; set; }
    public int BarbeiroId { get; set; }
    public Barbeiro Barbeiro { get; set; } = null!;
    public DayOfWeek DiaSemana { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
    public bool Disponivel { get; set; } = true;
    public int IntervaloMinutos { get; set; } = 30;
    public TimeSpan? AlmocoInicio { get; set; }
    public TimeSpan? AlmocoFim { get; set; }
}
