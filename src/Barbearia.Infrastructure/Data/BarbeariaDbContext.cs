using Barbearia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Infrastructure.Data;

public class BarbeariaDbContext : DbContext
{
    public BarbeariaDbContext(DbContextOptions<BarbeariaDbContext> options) : base(options) { }

    public DbSet<Barbeiro> Barbeiros => Set<Barbeiro>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<BarbeiroServico> BarbeiroServicos => Set<BarbeiroServico>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<HorarioBarbeiro> HorariosBarbeiro => Set<HorarioBarbeiro>();
    public DbSet<BloqueioAgenda> BloquiosAgenda => Set<BloqueioAgenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Barbeiro>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(20);
            e.Property(x => x.FotoUrl).HasMaxLength(500);
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Servico>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(500);
            e.Property(x => x.Preco).HasPrecision(10, 2);
        });

        modelBuilder.Entity<BarbeiroServico>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.BarbeiroId, x.ServicoId }).IsUnique();
            e.HasOne(x => x.Barbeiro).WithMany(b => b.BarbeiroServicos).HasForeignKey(x => x.BarbeiroId);
            e.HasOne(x => x.Servico).WithMany(s => s.BarbeiroServicos).HasForeignKey(x => x.ServicoId);
        });

        modelBuilder.Entity<Reserva>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClienteNome).HasMaxLength(100).IsRequired();
            e.Property(x => x.ClienteEmail).HasMaxLength(150).IsRequired();
            e.Property(x => x.ClienteTelefone).HasMaxLength(20).IsRequired();
            e.Property(x => x.CodigoConfirmacao).HasMaxLength(20).IsRequired();
            e.Property(x => x.Observacoes).HasMaxLength(500);
            e.Property(x => x.MotivoCancelamento).HasMaxLength(500);
            e.HasOne(x => x.Barbeiro).WithMany(b => b.Reservas).HasForeignKey(x => x.BarbeiroId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Servico).WithMany(s => s.Reservas).HasForeignKey(x => x.ServicoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HorarioBarbeiro>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Barbeiro).WithMany(b => b.Horarios).HasForeignKey(x => x.BarbeiroId);
        });

        modelBuilder.Entity<BloqueioAgenda>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Barbeiro).WithMany(b => b.Bloqueios).HasForeignKey(x => x.BarbeiroId);
        });

        // Seed data
        // IMPORTANTE: Hash estático pré-calculado para "admin123".
        // Nunca chamar BCrypt.HashPassword() aqui — gera salt aleatório a cada build,
        // causando PendingModelChangesWarning no EF Core.
        // Para regenerar: executar BCrypt.Net.BCrypt.HashPassword("admin123") uma vez e colar o resultado aqui.
        const string senhaAdminHash = "$2a$11$zSpNaUfBnGYJdqR5xSJTOeTLm3Mg5B7JOnBNRDJpHiHVnLaBOmT4a";

        modelBuilder.Entity<Barbeiro>().HasData(
            new Barbeiro
            {
                Id = 1,
                Nome = "Admin",
                Email = "admin@barbearia.com",
                Telefone = "(11) 99999-0000",
                Ativo = true,
                AgendaAberta = true,
                SenhaHash = senhaAdminHash,
                DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Servico>().HasData(
            new Servico { Id = 1, Nome = "Corte de Cabelo", Descricao = "Corte masculino completo", DuracaoMinutos = 30, Preco = 45.00m, Ativo = true, DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Servico { Id = 2, Nome = "Barba", Descricao = "Aparação e modelagem de barba", DuracaoMinutos = 20, Preco = 30.00m, Ativo = true, DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Servico { Id = 3, Nome = "Corte + Barba", Descricao = "Corte completo + barba", DuracaoMinutos = 50, Preco = 65.00m, Ativo = true, DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Servico { Id = 4, Nome = "Sobrancelha", Descricao = "Design de sobrancelha masculina", DuracaoMinutos = 15, Preco = 15.00m, Ativo = true, DataCriacao = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Default schedule for admin barber (Mon-Sat, 9h-19h)
        var diasUteis = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        int hId = 1;
        foreach (var dia in diasUteis)
        {
            modelBuilder.Entity<HorarioBarbeiro>().HasData(new HorarioBarbeiro
            {
                Id = hId++,
                BarbeiroId = 1,
                DiaSemana = dia,
                HoraInicio = new TimeSpan(9, 0, 0),
                HoraFim = new TimeSpan(19, 0, 0),
                Disponivel = true,
                IntervaloMinutos = 30
            });
        }
        modelBuilder.Entity<HorarioBarbeiro>().HasData(new HorarioBarbeiro
        {
            Id = hId,
            BarbeiroId = 1,
            DiaSemana = DayOfWeek.Saturday,
            HoraInicio = new TimeSpan(9, 0, 0),
            HoraFim = new TimeSpan(16, 0, 0),
            Disponivel = true,
            IntervaloMinutos = 30
        });
    }
}