using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthorizationService.Models;
using AuthorizationService.Service.IService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationService.Service;
public class JwtTokenGeneratorService : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions;
    private static RSA _rsa = RSA.Create();
    public JwtTokenGeneratorService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public static string GenerateSecureSecret()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var keyBytes = new byte[32];
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }
    }

    public string GenerateToken(AppUser applicationUser, UserRole role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,applicationUser.Id),
                new Claim(JwtRegisteredClaimNames.Name,applicationUser.UserName),
                new Claim(ClaimTypes.Role, role.ToString())
            };


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = _jwtOptions.Audience,
            Issuer = _jwtOptions.Issuer,
            Subject = new ClaimsIdentity(claimList),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(_rsa), SecurityAlgorithms.RsaSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    public static RsaSecurityKey GetRsaSecurityKey()
    {
        return new RsaSecurityKey(_rsa);
    }
    public string GetPublicKey()
    {
        var publicKey = _rsa.ExportSubjectPublicKeyInfo();
        return Convert.ToBase64String(publicKey);
    }
}
