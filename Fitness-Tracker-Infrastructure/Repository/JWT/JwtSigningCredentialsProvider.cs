using Fitness_Tracker_Application.Features.Users.JWT;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Fitness_Tracker_Infrastructure.Repository.JWT
{
    public sealed class JwtSigningCredentialsProvider: IJwtSigningCredentialsProvider, IDisposable
    {
        private readonly RSA _rsa;

        public SigningCredentials Credentials { get; }

        public string Issuer { get; }
        public string Audience { get; }

        public JwtSigningCredentialsProvider(IOptions<JwtConfigDTO> configuration)
        {
            var privatePem = File.ReadAllText(
                configuration.Value.PrivateKeyPath);

            _rsa = RSA.Create();

            try
            {
                _rsa.ImportFromPem(privatePem);

                Credentials = new SigningCredentials(
                    new RsaSecurityKey(_rsa),
                    SecurityAlgorithms.RsaSha256);
            }
            catch
            {
                _rsa.Dispose();
                throw;
            }

            Issuer = configuration.Value.Issuer;
            Audience = configuration.Value.Audience;
        }

        public void Dispose()
        {
            _rsa.Dispose();
        }
    }
}
