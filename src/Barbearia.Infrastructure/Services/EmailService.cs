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

    // ─── Cabeçalho e rodapé reutilizáveis ───────────────────────────────────

    private string CabecalhoEmail()
    {
        var baseUrl = _config["App:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
        var logoUrl = string.IsNullOrEmpty(baseUrl) ? "" : $"{baseUrl}/images/logo.jpeg";

        var logo = string.IsNullOrEmpty(logoUrl)
            ? """<div style="font-size:64px;line-height:1;">✂</div>"""
            : $"""<img src="{logoUrl}" alt="Logo" style="width:100px;height:100px;object-fit:contain;display:block;margin:0 auto 8px;" />""";

        return $"""
            <!DOCTYPE html>
            <html lang="pt">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background:#0d0d0d;font-family:Arial,sans-serif;">
            <table width="100%" cellpadding="0" cellspacing="0" style="background:#0d0d0d;padding:32px 16px;">
              <tr><td align="center">
                <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background:#1a1a1a;border-radius:12px;overflow:hidden;border:1px solid #2a2a2a;">

                  <!-- CABEÇALHO -->
                  <tr>
                    <td style="background:linear-gradient(180deg,#111 0%,#1a1a1a 100%);padding:36px 32px 28px;text-align:center;border-bottom:1px solid #2a2a2a;">
                      {logo}
                    </td>
                  </tr>
            """;
    }

    private static string RodapeEmail()
    {
        return """
                  <!-- RODAPÉ -->
                  <tr>
                    <td style="padding:24px 32px;text-align:center;border-top:1px solid #2a2a2a;">
                      <p style="margin:0 0 4px;color:#606060;font-size:12px;">Guarde este e-mail para gerir a sua marcação.</p>
                      <p style="margin:0;color:#404040;font-size:11px;">© Barbearia Profissional — Todos os direitos reservados</p>
                    </td>
                  </tr>

                </table>
              </td></tr>
            </table>
            </body>
            </html>
            """;
    }

    private static string LinhaDetalhe(string label, string valor, bool destaque = false)
    {
        var cor = destaque ? "#c9963f" : "#f0f0f0";
        return $"""
            <tr>
              <td style="padding:10px 0;color:#909090;font-size:14px;border-bottom:1px solid #2a2a2a;width:40%;">{label}</td>
              <td style="padding:10px 0;color:{cor};font-weight:{(destaque ? "700" : "500")};font-size:14px;border-bottom:1px solid #2a2a2a;">{valor}</td>
            </tr>
            """;
    }

    private static string CaixaDetalheReserva(Reserva reserva)
    {
        return $"""
            <tr>
              <td style="padding:0 32px 24px;">
                <table width="100%" cellpadding="0" cellspacing="0" style="background:#242424;border-radius:8px;padding:20px 24px;">
                  <tr><td colspan="2" style="padding-bottom:12px;">
                    <span style="color:#c9963f;font-size:12px;text-transform:uppercase;letter-spacing:2px;font-weight:700;">Detalhes da Marcação</span>
                  </td></tr>
                  {LinhaDetalhe("Barbeiro", reserva.Barbeiro?.Nome ?? "—")}
                  {LinhaDetalhe("Serviço", reserva.Servico?.Nome ?? "—")}
                  {LinhaDetalhe("Data / Hora", reserva.DataHora.ToString("dd/MM/yyyy 'às' HH:mm"))}
                  {LinhaDetalhe("Duração", $"{reserva.Servico?.DuracaoMinutos ?? 0} min")}
                  {LinhaDetalhe("Valor", $"€ {reserva.Servico?.Preco:F2}", destaque: true)}
                  {(string.IsNullOrEmpty(reserva.Observacoes) ? "" : LinhaDetalhe("Observações", reserva.Observacoes))}
                </table>
              </td>
            </tr>
            """;
    }

    private static string CaixaCodigoConfirmacao(string codigo)
    {
        return $"""
            <tr>
              <td style="padding:0 32px 28px;">
                <table width="100%" cellpadding="0" cellspacing="0" style="background:#c9963f;border-radius:8px;padding:20px 24px;text-align:center;">
                  <tr>
                    <td>
                      <p style="margin:0 0 4px;color:#000;font-size:11px;text-transform:uppercase;letter-spacing:2px;font-weight:600;">Código de Confirmação</p>
                      <p style="margin:0 0 6px;color:#000;font-size:32px;font-weight:800;letter-spacing:6px;font-family:monospace;">{codigo}</p>
                      <p style="margin:0;color:#3d2a00;font-size:12px;">Use este código para alterar ou cancelar a sua marcação</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
            """;
    }

    // ─── Templates ──────────────────────────────────────────────────────────

    public async Task EnviarConfirmacaoEmailAsync(Reserva reserva)
    {
        var html = CabecalhoEmail() + $"""

                  <!-- TÍTULO -->
                  <tr>
                    <td style="padding:28px 32px 8px;text-align:center;">
                      <div style="font-size:32px;margin-bottom:8px;">✅</div>
                      <h1 style="margin:0 0 4px;color:#c9963f;font-size:22px;font-weight:700;">Marcação Confirmada!</h1>
                      <p style="margin:0;color:#909090;font-size:14px;">Olá, <strong style="color:#f0f0f0;">{reserva.ClienteNome}</strong> — estamos à sua espera.</p>
                    </td>
                  </tr>

                  <!-- SEPARADOR -->
                  <tr><td style="padding:16px 32px;"><div style="height:1px;background:#2a2a2a;"></div></td></tr>

                  {CaixaDetalheReserva(reserva)}
                  {CaixaCodigoConfirmacao(reserva.CodigoConfirmacao)}

                """ + RodapeEmail();

        await EnviarEmailAsync(reserva.ClienteEmail,
            $"✅ Marcação Confirmada — {reserva.DataHora:dd/MM 'às' HH:mm}", html);
    }

    public async Task EnviarCancelamentoEmailAsync(Reserva reserva)
    {
        var motivoHtml = string.IsNullOrEmpty(reserva.MotivoCancelamento)
            ? ""
            : $"""
              <tr>
                <td style="padding:0 32px 16px;">
                  <div style="background:#2a1a1a;border-left:3px solid #c0392b;border-radius:4px;padding:14px 16px;">
                    <p style="margin:0;color:#c0392b;font-size:12px;text-transform:uppercase;letter-spacing:1px;font-weight:600;">Motivo do Cancelamento</p>
                    <p style="margin:4px 0 0;color:#f0f0f0;font-size:14px;">{reserva.MotivoCancelamento}</p>
                  </div>
                </td>
              </tr>
              """;

        var html = CabecalhoEmail() + $"""

                  <!-- TÍTULO -->
                  <tr>
                    <td style="padding:28px 32px 8px;text-align:center;">
                      <div style="font-size:32px;margin-bottom:8px;">❌</div>
                      <h1 style="margin:0 0 4px;color:#c0392b;font-size:22px;font-weight:700;">Marcação Cancelada</h1>
                      <p style="margin:0;color:#909090;font-size:14px;">Olá, <strong style="color:#f0f0f0;">{reserva.ClienteNome}</strong> — a sua marcação foi cancelada.</p>
                    </td>
                  </tr>

                  <!-- SEPARADOR -->
                  <tr><td style="padding:16px 32px;"><div style="height:1px;background:#2a2a2a;"></div></td></tr>

                  {CaixaDetalheReserva(reserva)}
                  {motivoHtml}

                  <!-- NOVA MARCAÇÃO -->
                  <tr>
                    <td style="padding:0 32px 28px;text-align:center;">
                      <p style="margin:0 0 12px;color:#909090;font-size:13px;">Deseja fazer uma nova marcação?</p>
                      <a href="{_config["App:BaseUrl"]?.TrimEnd('/') ?? "#"}/reservar"
                         style="display:inline-block;background:#c9963f;color:#000;font-weight:700;font-size:14px;padding:12px 28px;border-radius:50px;text-decoration:none;letter-spacing:1px;text-transform:uppercase;">
                        ✂ Fazer Nova Marcação
                      </a>
                    </td>
                  </tr>

                """ + RodapeEmail();

        await EnviarEmailAsync(reserva.ClienteEmail,
            $"❌ Marcação Cancelada — {reserva.DataHora:dd/MM 'às' HH:mm}", html);
    }

    public async Task EnviarAlteracaoEmailAsync(Reserva reserva)
    {
        var html = CabecalhoEmail() + $"""

                  <!-- TÍTULO -->
                  <tr>
                    <td style="padding:28px 32px 8px;text-align:center;">
                      <div style="font-size:32px;margin-bottom:8px;">✏️</div>
                      <h1 style="margin:0 0 4px;color:#c9963f;font-size:22px;font-weight:700;">Marcação Alterada</h1>
                      <p style="margin:0;color:#909090;font-size:14px;">Olá, <strong style="color:#f0f0f0;">{reserva.ClienteNome}</strong> — a sua marcação foi atualizada.</p>
                    </td>
                  </tr>

                  <!-- SEPARADOR -->
                  <tr><td style="padding:16px 32px;"><div style="height:1px;background:#2a2a2a;"></div></td></tr>

                  {CaixaDetalheReserva(reserva)}
                  {CaixaCodigoConfirmacao(reserva.CodigoConfirmacao)}

                """ + RodapeEmail();

        await EnviarEmailAsync(reserva.ClienteEmail,
            $"✏️ Marcação Alterada — {reserva.DataHora:dd/MM 'às' HH:mm}", html);
    }

    public async Task EnviarLembreteAsync(Reserva reserva)
    {
        var html = CabecalhoEmail() + $"""

                  <!-- TÍTULO -->
                  <tr>
                    <td style="padding:28px 32px 8px;text-align:center;">
                      <div style="font-size:32px;margin-bottom:8px;">⏰</div>
                      <h1 style="margin:0 0 4px;color:#c9963f;font-size:22px;font-weight:700;">Lembrete de Marcação</h1>
                      <p style="margin:0;color:#909090;font-size:14px;">Olá, <strong style="color:#f0f0f0;">{reserva.ClienteNome}</strong> — a sua marcação é <strong style="color:#c9963f;">amanhã</strong>!</p>
                    </td>
                  </tr>

                  <!-- SEPARADOR -->
                  <tr><td style="padding:16px 32px;"><div style="height:1px;background:#2a2a2a;"></div></td></tr>

                  {CaixaDetalheReserva(reserva)}

                  <!-- INFO EXTRA -->
                  <tr>
                    <td style="padding:0 32px 28px;text-align:center;">
                      <div style="background:#1a1500;border:1px solid #3d2a00;border-radius:8px;padding:16px 20px;">
                        <p style="margin:0;color:#c9963f;font-size:13px;">💡 Chegue com alguns minutos de antecedência.</p>
                        <p style="margin:6px 0 0;color:#909090;font-size:12px;">Código: <strong style="color:#f0f0f0;font-family:monospace;letter-spacing:2px;">{reserva.CodigoConfirmacao}</strong></p>
                      </div>
                    </td>
                  </tr>

                """ + RodapeEmail();

        await EnviarEmailAsync(reserva.ClienteEmail,
            $"⏰ Lembrete — A sua marcação é amanhã às {reserva.DataHora:HH:mm}", html);
    }

    // ─── WhatsApp (stub) ────────────────────────────────────────────────────

    public Task EnviarConfirmacaoWhatsAppAsync(Reserva reserva) => Task.CompletedTask;
    public Task EnviarCancelamentoWhatsAppAsync(Reserva reserva) => Task.CompletedTask;

    // ─── Envio SMTP ─────────────────────────────────────────────────────────

    private async Task EnviarEmailAsync(string destinatario, string assunto, string htmlBody)
    {
        var host = _config["Email:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Email:Port"] ?? "587");
        var usuario = _config["Email:Usuario"] ?? string.Empty;
        var senha = _config["Email:Senha"] ?? string.Empty;
        var nomeRemetente = _config["Email:NomeRemetente"] ?? "Barbearia";

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
