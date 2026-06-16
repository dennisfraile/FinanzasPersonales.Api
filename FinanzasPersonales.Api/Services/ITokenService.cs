using FinanzasPersonales.Api.Models;
using Microsoft.AspNetCore.Identity;
using Google.Apis.Auth;

namespace FinanzasPersonales.Api.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(IdentityUser user, string? fotoUrl);
        Task<string> IssueRefreshTokenAsync(string userId);
        Task<(string userId, string newRefreshRaw)?> RotateRefreshTokenAsync(string rawRefreshToken);
        Task RevokeAsync(string rawRefreshToken);
    }
}
