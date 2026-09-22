using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Dominio;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Aplicacion.Servicios;

public interface ITokenService
{
    (string Token, DateTime ExpiraEn) Generar(Usuario usuario);
}

/// <summary>Emite tokens JWT firmados con HMAC-SHA256.</summary>
public class TokenService(IOptions<JwtOptions> opciones) : ITokenService
{
    private readonly JwtOptions _jwt = opciones.Value;

    public (string Token, DateTime ExpiraEn) Generar(Usuario usuario)
    {
        var expiraEn = DateTime.UtcNow.AddMinutes(_jwt.MinutosDeVigencia);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Name, usuario.NombreCompleto),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEn,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
    }
}
