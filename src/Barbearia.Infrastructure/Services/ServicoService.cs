using Barbearia.Application.Interfaces;
using Barbearia.Domain.Entities;
using Barbearia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Services;

public class ServicoService : IServicoService
{
    private readonly BarbeariaDbContext _context;
    public ServicoService(BarbeariaDbContext context) => _context = context;

    public async Task<List<Servico>> ListarTodosAsync(bool apenasAtivos = false)
    {
        var q = _context.Servicos.AsQueryable();
        if (apenasAtivos) q = q.Where(s => s.Ativo);
        return await q.OrderBy(s => s.Nome).ToListAsync();
    }

    public async Task<Servico?> ObterPorIdAsync(int id) => await _context.Servicos.FindAsync(id);

    public async Task<Servico> CriarAsync(Servico servico)
    {
        servico.DataCriacao = DateTime.UtcNow;
        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync();
        return servico;
    }

    public async Task<Servico> AtualizarAsync(Servico servico)
    {
        var e = await _context.Servicos.FindAsync(servico.Id) ?? throw new InvalidOperationException("Serviço não encontrado.");
        e.Nome = servico.Nome; e.Descricao = servico.Descricao; e.DuracaoMinutos = servico.DuracaoMinutos; e.Preco = servico.Preco; e.Ativo = servico.Ativo;
        await _context.SaveChangesAsync();
        return e;
    }

    public async Task AtivarDesativarAsync(int id, bool ativo)
    {
        var s = await _context.Servicos.FindAsync(id) ?? throw new InvalidOperationException("Serviço não encontrado.");
        s.Ativo = ativo;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Servico>> ListarPorBarbeiroAsync(int barbeiroId)
        => await _context.BarbeiroServicos
            .Where(bs => bs.BarbeiroId == barbeiroId).Include(bs => bs.Servico)
            .Where(bs => bs.Servico.Ativo).Select(bs => bs.Servico).OrderBy(s => s.Nome).ToListAsync();

    public async Task AssociarServicosAsync(int barbeiroId, List<int> servicoIds)
    {
        var existentes = await _context.BarbeiroServicos.Where(bs => bs.BarbeiroId == barbeiroId).ToListAsync();
        _context.BarbeiroServicos.RemoveRange(existentes);
        foreach (var sId in servicoIds.Distinct())
            _context.BarbeiroServicos.Add(new BarbeiroServico { BarbeiroId = barbeiroId, ServicoId = sId });
        await _context.SaveChangesAsync();
    }
}
