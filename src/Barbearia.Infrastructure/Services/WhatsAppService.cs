using Barbearia.Application.Interfaces;
using Barbearia.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Barbearia.Infrastructure.Services;

public class WhatsAppService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(IConfiguration config, ILogger<WhatsAppService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task EnviarMensagemAsync(string telefone, string mensagem)
    {
        var accountSid = _config["Twilio:AccountSid"];
        var authToken = _config["Twilio:AuthToken"];
        var whatsappFrom = _config["Twilio:WhatsAppFrom"] ?? "whatsapp:+14155238886";

        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
        {
            _logger.LogWarning("Twilio não configurado. WhatsApp não enviado.");
            return;
        }

        TwilioClient.Init(accountSid, authToken);

        var numero = FormatarTelefone(telefone);
        if (string.IsNullOrEmpty(numero)) return;

        await MessageResource.CreateAsync(
            body: mensagem,
            from: new Twilio.Types.PhoneNumber(whatsappFrom),
            to: new Twilio.Types.PhoneNumber($"whatsapp:{numero}")
        );
    }

    public async Task EnviarConfirmacaoAsync(Reserva reserva)
    {
        var msg = $"""
            ✂ *BarberShop* - Reserva Confirmada!

            Olá, *{reserva.ClienteNome}*!

            📅 *Data:* {reserva.DataHora:dd/MM/yyyy}
            🕐 *Horário:* {reserva.DataHora:HH:mm}
            💈 *Barbeiro:* {reserva.Barbeiro?.Nome}
            ✂ *Serviço:* {reserva.Servico?.Nome}
            💰 *Valor:* R$ {reserva.Servico?.Preco:F2}

            🔑 *Código:* {reserva.CodigoConfirmacao}

            Use o código para alterar ou cancelar.
            Até logo!
            """;

        await EnviarMensagemAsync(reserva.ClienteTelefone, msg);
    }

    public async Task EnviarCancelamentoAsync(Reserva reserva)
    {
        var msg = $"""
            ✂ *BarberShop* - Reserva Cancelada

            Olá, *{reserva.ClienteNome}*.
            Sua reserva do dia *{reserva.DataHora:dd/MM/yyyy HH:mm}* foi cancelada.

            Para remarcar acesse nosso site.
            """;

        await EnviarMensagemAsync(reserva.ClienteTelefone, msg);
    }

    private static string FormatarTelefone(string telefone)
    {
        var apenas = new string(telefone.Where(char.IsDigit).ToArray());
        if (apenas.Length == 11) return $"+55{apenas}";
        if (apenas.Length == 13 && apenas.StartsWith("55")) return $"+{apenas}";
        return string.Empty;
    }
}
