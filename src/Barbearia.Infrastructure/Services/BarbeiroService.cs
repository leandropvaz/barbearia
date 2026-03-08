using Barbearia.Application.Interfaces;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Services;

public class BarbeiroService : IBarbeiroService
{
    private readonly BarbeariaDbContext _context;
    private readonly IArquivoService _arquivoService;

    public BarbeiroService(BarbeariaDbContext context, IArquivoService arquivoService)
    {
        _context = context;
        _arquivoService = arquivoService;
    }

    public async Task<List<Barbeiro>> ListarTodosAsync(bool apenasAtivos = false)
    {
        var query = _context.Barbeiros
            .Include(b => b.BarbeiroServicos).ThenInclude(bs => bs.Servico)
            .AsQueryable();
        if (apenasAtivos) query = query.Where(b => b.Ativo);
        return await query.OrderBy(b => b.Nome).ToListAsync();
    }

    public async Task<Barbeiro?> ObterPorIdAsync(int id)
        => await _context.Barbeiros
            .Include(b => b.BarbeiroServicos).ThenInclude(bs => bs.Servico)
            .Include(b => b.Horarios)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Barbeiro?> ObterPorEmailAsync(string email)
        => await _context.Barbeiros.FirstOrDefaultAsync(b => b.Email == email.ToLower());

    public async Task<Barbeiro> CriarAsync(Barbeiro barbeiro, string senha)
    {
        barbeiro.Email = barbeiro.Email.ToLower().Trim();
        barbeiro.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        barbeiro.DataCriacao = DateTime.UtcNow;
        _context.Barbeiros.Add(barbeiro);
        await _context.SaveChangesAsync();

        var diasUteis = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        foreach (var dia in diasUteis)
            _context.HorariosBarbeiro.Add(new HorarioBarbeiro { BarbeiroId = barbeiro.Id, DiaSemana = dia, HoraInicio = new TimeSpan(9, 0, 0), HoraFim = new TimeSpan(19, 0, 0), Disponivel = true, IntervaloMinutos = 30 });
        _context.HorariosBarbeiro.Add(new HorarioBarbeiro { BarbeiroId = barbeiro.Id, DiaSemana = DayOfWeek.Saturday, HoraInicio = new TimeSpan(9, 0, 0), HoraFim = new TimeSpan(16, 0, 0), Disponivel = true, IntervaloMinutos = 30 });
        await _context.SaveChangesAsync();
        return barbeiro;
    }

    public async Task<Barbeiro> AtualizarAsync(Barbeiro barbeiro, string? novaSenha = null)
    {
        var existente = await _context.Barbeiros.FindAsync(barbeiro.Id) ?? throw new InvalidOperationException("Barbeiro não encontrado.");
        existente.Nome = barbeiro.Nome;
        existente.Telefone = barbeiro.Telefone;
        existente.AgendaAberta = barbeiro.AgendaAberta;
        existente.Ativo = barbeiro.Ativo;
        if (!string.IsNullOrWhiteSpace(novaSenha)) existente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        await _context.SaveChangesAsync();
        return existente;
    }

    public async Task AtivarDesativarAsync(int id, bool ativo)
    {
        var b = await _context.Barbeiros.FindAsync(id) ?? throw new InvalidOperationException("Barbeiro não encontrado.");
        b.Ativo = ativo;
        await _context.SaveChangesAsync();
    }

    public async Task AbrirFecharAgendaAsync(int id, bool aberta)
    {
        var b = await _context.Barbeiros.FindAsync(id) ?? throw new InvalidOperationException("Barbeiro não encontrado.");
        b.AgendaAberta = aberta;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidarSenhaAsync(string email, string senha)
    {
        var b = await ObterPorEmailAsync(email);
        return b != null && BCrypt.Net.BCrypt.Verify(senha, b.SenhaHash);
    }

    public async Task<string?> AtualizarFotoAsync(int id, Stream fotoStream, string nomeArquivo)
    {
        var b = await _context.Barbeiros.FindAsync(id) ?? throw new InvalidOperationException("Barbeiro não encontrado.");
        if (!string.IsNullOrEmpty(b.FotoUrl)) await _arquivoService.DeleteAsync(b.FotoUrl);
        var ext = Path.GetExtension(nomeArquivo);
        var url = await _arquivoService.UploadAsync(fotoStream, $"barbeiro-{id}-{Guid.NewGuid():N}{ext}");
        b.FotoUrl = url;
        await _context.SaveChangesAsync();
        return url;
    }

    public async Task ExcluirFotoAsync(int id)
    {
        var b = await _context.Barbeiros.FindAsync(id) ?? throw new InvalidOperationException("Barbeiro não encontrado.");
        if (!string.IsNullOrEmpty(b.FotoUrl)) { await _arquivoService.DeleteAsync(b.FotoUrl); b.FotoUrl = null; await _context.SaveChangesAsync(); }
    }

    public async Task SalvarHorariosAsync(int barbeiroId, List<HorarioBarbeiro> horarios)
    {
        var existentes = await _context.HorariosBarbeiro.Where(h => h.BarbeiroId == barbeiroId).ToListAsync();
        foreach (var e in existentes)
        {
            var novo = horarios.FirstOrDefault(h => h.Id == e.Id);
            if (novo == null) continue;
            e.Disponivel = novo.Disponivel;
            e.HoraInicio = novo.HoraInicio;
            e.HoraFim = novo.HoraFim;
            e.IntervaloMinutos = novo.IntervaloMinutos;
            e.AlmocoInicio = novo.AlmocoInicio;
            e.AlmocoFim = novo.AlmocoFim;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<BloqueioAgenda>> ListarBloqueiosAsync(int barbeiroId, DateTime data)
        => await _context.BloquiosAgenda
            .Where(b => b.BarbeiroId == barbeiroId && b.DataInicio.Date == data.Date)
            .ToListAsync();

    public async Task BloquearSlotAsync(int barbeiroId, DateTime inicio, DateTime fim, string? motivo = null)
    {
        _context.BloquiosAgenda.Add(new BloqueioAgenda { BarbeiroId = barbeiroId, DataInicio = inicio, DataFim = fim, Motivo = motivo });
        await _context.SaveChangesAsync();
    }

    public async Task DesbloquearSlotAsync(int id)
    {
        var b = await _context.BloquiosAgenda.FindAsync(id);
        if (b != null) { _context.BloquiosAgenda.Remove(b); await _context.SaveChangesAsync(); }
    }
}
