# ✂ BarberShop - Sistema de Agendamento

Sistema completo de agendamento para barbearia, desenvolvido com **Blazor Server (.NET 9)** + **Azure**.

## Estrutura do Projeto

```
Barbearia/
├── src/
│   ├── Barbearia.Domain/          # Entidades e Enums
│   ├── Barbearia.Application/     # Interfaces de serviço
│   ├── Barbearia.Infrastructure/  # EF Core, Azure Blob, Email, WhatsApp
│   └── Barbearia.Web/             # Blazor Server (UI)
```

## Módulos

### Admin (`/admin`)
- **Login**: `/admin/login` (email + senha)
- **Dashboard**: visão geral do dia
- **Barbeiros**: cadastro, foto, ativar/desativar, abrir/fechar agenda
- **Serviços**: cadastro e associação a barbeiros
- **Reservas**: gerenciar todas as reservas
- **Agenda**: visão de horários com slots disponíveis/ocupados
- **Minha Agenda**: visão pessoal do barbeiro logado

### Cliente (`/`)
- **Reservar**: fluxo em 4 passos (barbeiro → serviço → data/hora → dados)
- **Minha Reserva**: `/minha-reserva/{codigo}` - alterar ou cancelar

## Configuração

### 1. Banco de Dados
Edite `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SEU_SERVIDOR;Database=BarbeariaDb;..."
}
```

Para **Azure SQL Database** (baixo custo - Serverless Basic):
```
Server=seu-servidor.database.windows.net;Database=BarbeariaDb;
User Id=admin;Password=SUA_SENHA;Encrypt=True;
```

### 2. E-mail (Gmail)
```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "Usuario": "seuemail@gmail.com",
  "Senha": "SUA_APP_PASSWORD",
  "NomeRemetente": "BarberShop"
}
```
> Crie uma App Password no Google: Conta → Segurança → Senhas de app

### 3. WhatsApp (Twilio)
1. Crie conta em [twilio.com](https://twilio.com)
2. Ative o Sandbox WhatsApp
3. Configure:
```json
"Twilio": {
  "AccountSid": "ACxxxxxxxxxxxxxxxx",
  "AuthToken": "xxxxxxxxxxxxxxxx",
  "WhatsAppFrom": "whatsapp:+14155238886"
}
```

### 4. Azure Blob Storage (fotos dos barbeiros)
```json
"Azure": {
  "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
  "StorageBaseUrl": "https://suaconta.blob.core.windows.net"
}
```

## Rodando Localmente

```bash
# Instalar ferramentas EF
dotnet tool install --global dotnet-ef

# Aplicar migrations (banco local)
dotnet ef database update --project src/Barbearia.Infrastructure --startup-project src/Barbearia.Web

# Rodar
dotnet run --project src/Barbearia.Web
```

Acesse: `https://localhost:5001`

**Login padrão**: `admin@barbearia.com` / `admin123`

## Deploy Azure (Baixo Custo)

### Infraestrutura estimada (~R$ 50-80/mês):
| Serviço | Plano | Custo |
|---------|-------|-------|
| Azure App Service | B1 Basic | ~$13/mês |
| Azure SQL Database | Serverless 1 vCore | ~$5-15/mês |
| Azure Blob Storage | Pay-as-you-go | ~$1/mês |
| Azure Communication / SendGrid | Free (100 emails/dia) | Grátis |

### Deploy via CLI:
```bash
# Login Azure
az login

# Criar grupo de recursos
az group create --name barbearia-rg --location brazilsouth

# Criar App Service Plan (B1)
az appservice plan create --name barbearia-plan --resource-group barbearia-rg --sku B1

# Criar Web App
az webapp create --name minha-barbearia --plan barbearia-plan --resource-group barbearia-rg --runtime "DOTNET:9"

# Publicar
dotnet publish src/Barbearia.Web -c Release -o ./publish
az webapp deploy --resource-group barbearia-rg --name minha-barbearia --src-path ./publish
```

### Configurar Connection String no Azure:
```bash
az webapp config connection-string set \
  --name minha-barbearia \
  --resource-group barbearia-rg \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=..."
```

## Personalização

- **Logo/Cores**: edite `wwwroot/app.css` (variáveis CSS `:root`)
- **Nome da barbearia**: busque por "BarberShop" nos arquivos `.razor`
- **Dias/horários padrão**: `BarbeariaDbContext.cs` → seção Seed

## Funcionalidades

- ✅ Cadastro de barbeiros com foto
- ✅ Ativar/desativar barbeiros
- ✅ Abrir/fechar agenda por barbeiro
- ✅ Horários configuráveis por dia da semana
- ✅ Cadastro de serviços com preço e duração
- ✅ Associação de serviços por barbeiro
- ✅ Reserva sem cadastro (apenas nome, tel, email)
- ✅ Verificação de conflitos de horário
- ✅ Código de confirmação único (8 chars)
- ✅ Email de confirmação com layout HTML
- ✅ WhatsApp via Twilio
- ✅ Cliente pode alterar e cancelar com código
- ✅ Admin pode gerenciar todas as reservas
- ✅ Barbeiro vê sua própria agenda
- ✅ Dashboard com estatísticas do dia
- ✅ Tema escuro profissional responsivo
