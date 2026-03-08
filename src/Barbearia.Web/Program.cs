using Barbearia.Application.Interfaces;
using Barbearia.Infrastructure.Data;
using Barbearia.Infrastructure.Services;
using Barbearia.Web.Components;
using Barbearia.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.Name = "BarbeariaAdmin";
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDbContext<BarbeariaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<INotificacaoService, EmailService>();
builder.Services.AddScoped<IArquivoService, LocalFileService>();
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddSingleton<LoginTokenService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BarbeariaDbContext>();
    db.Database.Migrate();
}

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapGet("/admin/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/admin/login");
});

app.MapGet("/admin/login/callback", async (string token, HttpContext ctx, LoginTokenService tokenService) =>
{
    var principal = tokenService.ConsumeToken(token);
    if (principal is null)
        return Results.Redirect("/admin/login");

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Redirect("/admin");
});

app.Run();