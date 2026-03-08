using System.Collections.Concurrent;
using System.Security.Claims;

namespace Barbearia.Web.Services;

public class LoginTokenService
{
    private readonly ConcurrentDictionary<string, (ClaimsPrincipal Principal, DateTime Expiry)> _tokens = new();

    public string GenerateToken(ClaimsPrincipal principal)
    {
        var token = Guid.NewGuid().ToString("N");
        _tokens[token] = (principal, DateTime.UtcNow.AddMinutes(2));
        return token;
    }

    public ClaimsPrincipal? ConsumeToken(string token)
    {
        if (_tokens.TryRemove(token, out var entry) && entry.Expiry > DateTime.UtcNow)
            return entry.Principal;
        return null;
    }
}
