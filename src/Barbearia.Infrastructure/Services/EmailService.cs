using Barbearia.Application.Interfaces;
using Barbearia.Domain.Entities;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Barbearia.Infrastructure.Services;

public class EmailService : INotificacaoService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task EnviarConfirmacaoEmailAsync(Reserva reserva)
    {
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#1a1a1a;color:#fff;padding:30px;border-radius:12px;">
              <div style="text-align:center;margin-bottom:24px;">
                <h1 style="color:#c9963f;font-size:28px;margin:0;">✂ BarberShop</h1>
                <p style="color:#999;margin:4px 0;">Reserva Confirmada!</p>
              </div>
              <div style="background:#2a2a2a;border-radius:8px;padding:20px;margin-bottom:16px;">
                <h2 style="color:#c9963f;margin-top:0;">Olá, {reserva.ClienteNome}!</h2>
                <p>Sua reserva foi confirmada com sucesso.</p>
                <table style="width:100%;border-collapse:collapse;">
                  <tr><td style="padding:8px 0;color:#999;">Barbeiro</td><td style="color:#fff;font-weight:bold;">{reserva.Barbeiro?.Nome}</td></tr>
                  <tr><td style="padding:8px 0;color:#999;">Serviço</td><td style="color:#fff;font-weight:bold;">{reserva.Servico?.Nome}</td></tr>
                  <tr><td style="padding:8px 0;color:#999;">Data/Hora</td><td style="color:#fff;font-weight:bold;">{reserva.DataHora:dd/MM/yyyy HH:mm}</td></tr>
                  <tr><td style="padding:8px 0;color:#999;">Valor</td><td style="color:#c9963f;font-weight:bold;">R$ {reserva.Servico?.Preco:F2}</td></tr>
                </table>
              </div>
              <div style="background:#c9963f;border-radius:8px;padding:16px;text-align:center;margin-bottom:16px;">
                <p style="margin:0;color:#000;font-size:12px;">Código de Confirmação</p>
                <p style="margin:4px 0;color:#000;font-size:28px;font-weight:bold;letter-spacing:4px;">{reserva.CodigoConfirmacao}</p>
                <p style="margin:0;color:#000;font-size:12px;">Use este código para alterar ou cancelar sua reserva</p>
              </div>
              <p style="color:#999;font-size:12px;text-align:center;">Guarde este e-mail. Até breve!</p>
            </div>
            """;

        await EnviarEmailAsync(reserva.ClienteEmail, $"Reserva Confirmada - {reserva.DataHora:dd/MM HH:mm}", html);
    }

    public async Task EnviarCancelamentoEmailAsync(Reserva reserva)
    {
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#1a1a1a;color:#fff;padding:30px;border-radius:12px;">
              <h1 style="color:#c9963f;">✂ BarberShop</h1>
              <h2>Reserva Cancelada</h2>
              <p>Olá, <strong>{reserva.ClienteNome}</strong>. Sua reserva do dia <strong>{reserva.DataHora:dd/MM/yyyy HH:mm}</strong> com <strong>{reserva.Barbeiro?.Nome}</strong> foi cancelada.</p>
              {(string.IsNullOrEmpty(reserva.MotivoCancelamento) ? "" : $"<p>Motivo: {reserva.MotivoCancelamento}</p>")}
              <p style="color:#999;">Para fazer uma nova reserva, acesse nosso site.</p>
            </div>
            """;

        await EnviarEmailAsync(reserva.ClienteEmail, "Reserva Cancelada - BarberShop", html);
    }

    public async Task EnviarAlteracaoEmailAsync(Reserva reserva)
    {
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#1a1a1a;color:#fff;padding:30px;border-radius:12px;">
              <h1 style="color:#c9963f;">✂ BarberShop</h1>
              <h2>Reserva Alterada</h2>
              <p>Olá, <strong>{reserva.ClienteNome}</strong>. Sua reserva foi atualizada:</p>
              <p><strong>Nova Data/Hora:</strong> {reserva.DataHora:dd/MM/yyyy HH:mm}</p>
              <p><strong>Serviço:</strong> {reserva.Servico?.Nome}</p>
              <p><strong>Código:</strong> {reserva.CodigoConfirmacao}</p>
            </div>
            """;

        await EnviarEmailAsync(reserva.ClienteEmail, "Reserva Alterada - BarberShop", html);
    }

    public async Task EnviarLembreteAsync(Reserva reserva)
    {
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#1a1a1a;color:#fff;padding:30px;border-radius:12px;">
              <h1 style="color:#c9963f;">✂ BarberShop</h1>
              <h2>Lembrete de Reserva</h2>
              <p>Olá, <strong>{reserva.ClienteNome}</strong>! Você tem uma reserva amanhã:</p>
              <p><strong>Horário:</strong> {reserva.DataHora:HH:mm}</p>
              <p><strong>Barbeiro:</strong> {reserva.Barbeiro?.Nome}</p>
              <p><strong>Serviço:</strong> {reserva.Servico?.Nome}</p>
            </div>
            """;

        await EnviarEmailAsync(reserva.ClienteEmail, "Lembrete - Sua reserva é amanhã!", html);
    }

    public Task EnviarConfirmacaoWhatsAppAsync(Reserva reserva) => Task.CompletedTask;
    public Task EnviarCancelamentoWhatsAppAsync(Reserva reserva) => Task.CompletedTask;

    private async Task EnviarEmailAsync(string destinatario, string assunto, string htmlBody)
    {
        var host = _config["Email:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Email:Port"] ?? "587");
        var usuario = _config["Email:Usuario"] ?? string.Empty;
        var senha = _config["Email:Senha"] ?? string.Empty;
        var nomeRemetente = _config["Email:NomeRemetente"] ?? "BarberShop";

        if (string.IsNullOrEmpty(usuario))
        {
            _logger.LogWarning("E-mail não enviado: Email:Usuario não configurado em appsettings.json.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(nomeRemetente, usuario));
        message.To.Add(MailboxAddress.Parse(destinatario));
        message.Subject = assunto;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(usuario, senha);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("E-mail enviado para {Destinatario}: {Assunto}", destinatario, assunto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {Destinatario}: {Assunto}", destinatario, assunto);
            throw;
        }
    }
}
