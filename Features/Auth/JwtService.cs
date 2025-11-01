using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace listly.Features.Auth
{
  public class JwtService
  {
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public string GenerateJwtToken(string uid)
    {
      // Leer configuración appsettings.json
      var secretKey = _configuration["Jwt:SecretKey"];
      var issuer = _configuration["Jwt:Issuer"];
      var audience = _configuration["Jwt:Audience"];
      var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

      // Crear los claims
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
        new Claim(ClaimTypes.NameIdentifier, uid),
        new Claim("uid", uid),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
      };

      // Crear el JWT
      var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
        signingCredentials: credentials
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}