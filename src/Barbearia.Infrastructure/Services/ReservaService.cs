using Barbearia.Application.Interfaces;
using Barbearia.Domain.Entities;
using Barbearia.Domain.Enums;
using Barbearia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Services;

public class ReservaService : IReservaService
{
    private readonly BarbeariaDbContext _context;
    private readonly INotificacaoService _notificacao;

    public ReservaService(BarbeariaDbContext context, INotificacaoService notificacao)
    {
        _context = context;
        _notificacao = notificacao;
    }

    public async Task<List<Reserva>> ListarPorBarbeiroAsync(int barbeiroId, DateTime? data = null)
    {
        var q = _context.Reservas.Include(r => r.Barbeiro).Include(r => r.Servico)
            .Where(r => r.BarbeiroId == barbeiroId);
        if (data.HasValue) q = q.Where(r => r.DataHora.Date == data.Value.Date);
        return await q.OrderBy(r => r.DataHora).ToListAsync();
    }

    public async Task<List<Reserva>> ListarTodasAsync(DateTime? data = null, StatusReserva? status = null)
    {
        var q = _context.Reservas.Include(r => r.Barbeiro).Include(r => r.Servico).AsQueryable();
        if (data.HasValue) q = q.Where(r => r.DataHora.Date == data.Value.Date);
        if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        return await q.OrderBy(r => r.DataHora).ToListAsync();
    }

    public async Task<Reserva?> ObterPorIdAsync(int id)
        => await _context.Reservas.Include(r => r.Barbeiro).Include(r => r.Servico).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Reserva?> ObterPorCodigoAsync(string codigo)
        => await _context.Reservas.Include(r => r.Barbeiro).Include(r => r.Servico).FirstOrDefaultAsync(r => r.CodigoConfirmacao == codigo.ToUpper());

    public async Task<Reserva> CriarAsync(Reserva reserva)
    {
        var b = await _context.Barbeiros.FindAsync(reserva.BarbeiroId) ?? throw new InvalidOperationException("Barbeiro não encontrado.");
        if (!b.Ativo || !b.AgendaAberta) throw new InvalidOperationException("Este barbeiro não está disponível.");

        var disponiveis = await ObterHorariosDisponiveisAsync(reserva.BarbeiroId, reserva.ServicoId, reserva.DataHora.Date);
        if (!disponiveis.Any(h => h == reserva.DataHora)) throw new InvalidOperationException("Horário não disponível. Por favor, escolha outro horário.");

        reserva.CodigoConfirmacao = GerarCodigo();
        reserva.Status = StatusReserva.Confirmada;
        reserva.DataCriacao = DateTime.UtcNow;
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();

        await _context.Entry(reserva).Reference(r => r.Barbeiro).LoadAsync();
        await _context.Entry(reserva).Reference(r => r.Servico).LoadAsync();

        _ = Task.Run(async () =>
        {
            try { await _notificacao.EnviarConfirmacaoEmailAsync(reserva); } catch { }
            try { await _notificacao.EnviarConfirmacaoWhatsAppAsync(reserva); } catch { }
        });
        return reserva;
    }

    public async Task<Reserva> AtualizarAsync(Reserva reserva)
    {
        var e = await _context.Reservas.Include(r => r.Barbeiro).Include(r => r.Servico).FirstOrDefaultAsync(r => r.Id == reserva.Id)
            ?? throw new InvalidOperationException("Reserva não encontrada.");
        if (e.Status == StatusReserva.Cancelada) throw new InvalidOperationException("Não é possível alterar uma reserva cancelada.");

        if (reserva.DataHora != e.DataHora || reserva.ServicoId != e.ServicoId)
        {
            var disponiveis = await ObterHorariosDisponiveisAsync(e.BarbeiroId, reserva.ServicoId, reserva.DataHora.Date);
            if (!disponiveis.Any(h => h == reserva.DataHora)) throw new InvalidOperationException("Horário não disponível.");
        }

        e.DataHora = reserva.DataHora;
        e.ServicoId = reserva.ServicoId;
        e.Observacoes = reserva.Observacoes;
        e.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _context.Entry(e).Reference(r => r.Servico).LoadAsync();

        _ = Task.Run(async () => { try { await _notificacao.EnviarAlteracaoEmailAsync(e); } catch { } });
        return e;
    }

    public async Task ConfirmarAsync(int id)
    {
        var r = await _context.Reservas.FindAsync(id) ?? throw new InvalidOperationException("Marcação não encontrada.");
        r.Status = StatusReserva.Confirmada;
        r.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task CancelarAsync(int id, string motivo)
    {
        var r = await _context.Reservas.Include(rv => rv.Barbeiro).Include(rv => rv.Servico).FirstOrDefaultAsync(rv => rv.Id == id)
            ?? throw new InvalidOperationException("Reserva não encontrada.");
        r.Status = StatusReserva.Cancelada;
        r.MotivoCancelamento = motivo;
        r.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try { await _notificacao.EnviarCancelamentoEmailAsync(r); } catch { }
            try { await _notificacao.EnviarCancelamentoWhatsAppAsync(r); } catch { }
        });
    }

    public async Task ConcluirAsync(int id)
    {
        var r = await _context.Reservas.FindAsync(id) ?? throw new InvalidOperationException("Reserva não encontrada.");
        r.Status = StatusReserva.Concluida;
        r.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, int servicoId, DateTime data)
    {
        var servico = await _context.Servicos.FindAsync(servicoId);
        if (servico == null) return new();

        var horario = await _context.HorariosBarbeiro.FirstOrDefaultAsync(h => h.BarbeiroId == barbeiroId && h.DiaSemana == data.DayOfWeek && h.Disponivel);
        if (horario == null) return new();

        var b = await _context.Barbeiros.FindAsync(barbeiroId);
        if (b == null || !b.Ativo || !b.AgendaAberta) return new();

        var diaInicio = data.Date;
        var diaFim = data.Date.AddDays(1);
        var bloqueios = await _context.BloquiosAgenda
            .Where(bl => bl.BarbeiroId == barbeiroId && bl.DataInicio >= diaInicio && bl.DataInicio < diaFim)
            .ToListAsync();

        var reservas = await _context.Reservas.Include(r => r.Servico)
            .Where(r => r.BarbeiroId == barbeiroId && r.DataHora.Date == data.Date && r.Status != StatusReserva.Cancelada)
            .ToListAsync();

        var slots = new List<DateTime>();
        var atual = data.Date.Add(horario.HoraInicio);
        var fim = data.Date.Add(horario.HoraFim);

        while (atual.AddMinutes(servico.DuracaoMinutos) <= fim)
        {
            var slotFim = atual.AddMinutes(servico.DuracaoMinutos);
            var ocupado = reservas.Any(r =>
            {
                var rFim = r.DataHora.AddMinutes(r.Servico?.DuracaoMinutos ?? 30);
                return atual < rFim && slotFim > r.DataHora;
            });
            var bloqueado = bloqueios.Any(bl => atual < bl.DataFim && slotFim > bl.DataInicio);
            var emAlmoco = horario.AlmocoInicio.HasValue && horario.AlmocoFim.HasValue &&
                atual < data.Date.Add(horario.AlmocoFim.Value) && slotFim > data.Date.Add(horario.AlmocoInicio.Value);
            if (!ocupado && !bloqueado && !emAlmoco && atual > DateTime.UtcNow.AddMinutes(30))
                slots.Add(atual);
            atual = atual.AddMinutes(horario.IntervaloMinutos);
        }
        return slots;
    }

    private static string GerarCodigo()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = new Random();
        return new string(Enumerable.Range(0, 8).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
    }
}
