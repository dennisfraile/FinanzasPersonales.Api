using FinanzasPersonales.Api.Services;
using Xunit;

public class TokenServiceTests
{
    [Fact]
    public void HashRefreshToken_IsDeterministic_AndNotPlaintext()
    {
        var raw = TokenService.GenerateRefreshTokenRaw();
        var h1 = TokenService.HashToken(raw);
        var h2 = TokenService.HashToken(raw);

        Assert.Equal(h1, h2);
        Assert.NotEqual(raw, h1);
        Assert.True(h1.Length <= 64);
    }

    [Fact]
    public void GenerateRefreshTokenRaw_ProducesUniqueValues()
    {
        var a = TokenService.GenerateRefreshTokenRaw();
        var b = TokenService.GenerateRefreshTokenRaw();
        Assert.NotEqual(a, b);
        Assert.True(a.Length >= 32);
    }
}
