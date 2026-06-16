using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinanzasPersonales.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly FinanzasDbContext _db;

        public TokenService(IConfiguration config, FinanzasDbContext db)
        {
            _config = config;
            _db = db;
        }

        public static string GenerateRefreshTokenRaw()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public static string HashToken(string raw)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(hash); // 44 chars
        }

        public string CreateAccessToken(IdentityUser user, string? fotoUrl)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var minutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 15;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (!string.IsNullOrEmpty(fotoUrl))
                claims.Add(new Claim("FotoUrl", fotoUrl));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> IssueRefreshTokenAsync(string userId)
        {
            var raw = GenerateRefreshTokenRaw();
            var days = int.TryParse(_config["Jwt:RefreshTokenDays"], out var d) ? d : 7;
            _db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = HashToken(raw),
                UserId = userId,
                ExpiresUtc = DateTime.UtcNow.AddDays(days)
            });
            await _db.SaveChangesAsync();
            return raw;
        }

        public async Task<(string userId, string newRefreshRaw)?> RotateRefreshTokenAsync(string rawRefreshToken)
        {
            var hash = HashToken(rawRefreshToken);
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);
            if (existing == null) return null;

            if (!existing.IsActive)
            {
                var userTokens = _db.RefreshTokens.Where(rt => rt.UserId == existing.UserId && rt.RevokedUtc == null);
                await userTokens.ForEachAsync(t => t.RevokedUtc = DateTime.UtcNow);
                await _db.SaveChangesAsync();
                return null;
            }

            var newRaw = GenerateRefreshTokenRaw();
            var days = int.TryParse(_config["Jwt:RefreshTokenDays"], out var d) ? d : 7;
            existing.RevokedUtc = DateTime.UtcNow;
            existing.ReplacedByTokenHash = HashToken(newRaw);
            _db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = existing.ReplacedByTokenHash,
                UserId = existing.UserId,
                ExpiresUtc = DateTime.UtcNow.AddDays(days)
            });
            await _db.SaveChangesAsync();
            return (existing.UserId, newRaw);
        }

        public async Task RevokeAsync(string rawRefreshToken)
        {
            var hash = HashToken(rawRefreshToken);
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);
            if (existing != null && existing.RevokedUtc == null)
            {
                existing.RevokedUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
