using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Features.Users.JWT;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
public class GenerateJwtTokenTests
{
    [Fact]
    public async Task Handle_ShouldReturnValidSignedToken_WithUserClaims()
    {
        using var privateRsa = RSA.Create(2048);

        var configuration = new JwtConfigDTO
        {
            Issuer = "test-issuer",
            Audience = "test-audience"
        };

        var provider = new TestSigningCredentialsProvider(
            new SigningCredentials(
                new RsaSecurityKey(privateRsa),
                SecurityAlgorithms.RsaSha256),
            configuration);

        var user = new UserDTO("test-user", Guid.NewGuid());

        var sut = new GenerateJwtToken(provider);

        using var publicRsa = RSA.Create();
        publicRsa.ImportParameters(privateRsa.ExportParameters(false));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(publicRsa),

            ValidateIssuer = true,
            ValidIssuer = configuration.Issuer,

            ValidateAudience = true,
            ValidAudience = configuration.Audience,

            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,

            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }
        };

        var tokenHandler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };

        var before = DateTime.UtcNow;

        var token = await sut.Handle(
            new GenerateJwtTokenCommand(user),
            CancellationToken.None);

        var after = DateTime.UtcNow;

        Assert.False(string.IsNullOrWhiteSpace(token));

        var principal = tokenHandler.ValidateToken(
            token,
            validationParameters,
            out var validatedToken);

        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);

        Assert.Equal(SecurityAlgorithms.RsaSha256, jwt.Header.Alg);

        Assert.Equal(
            user.Id.ToString(),
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);

        Assert.Equal(
            user.Login,
            principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value);
        
        Assert.Equal(
            configuration.Issuer,
            principal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value);

        Assert.Equal(
            configuration.Audience,
            principal.FindFirst(JwtRegisteredClaimNames.Aud)?.Value);   

        Assert.InRange(
            jwt.ValidTo,
            before.AddMinutes(15).AddSeconds(-1),
            after.AddMinutes(15));
    }

    private sealed class TestSigningCredentialsProvider: IJwtSigningCredentialsProvider
    {
        public SigningCredentials Credentials { get; }

        public string Issuer { get; }

        public string Audience { get; }

        public TestSigningCredentialsProvider(SigningCredentials credentials, JwtConfigDTO config)
        {
            Issuer = config.Issuer;
            Audience = config.Audience;
            Credentials = credentials;
        }
    }
}