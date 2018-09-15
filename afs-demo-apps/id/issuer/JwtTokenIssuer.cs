using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using afs.jwt.abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using OpenSSL.PrivateKeyDecoder;

namespace afs.jwt.issuer
{
    /// <summary>JWT token issuer</summary>
    /// <remarks>
    /// Supported signing algorithms:
    ///  - hs256 - symmetric
    ///  - rsa-sha256 - asymmetric
    /// </remarks>
    public class JwtTokenIssuer : ITokenIssuer
    {
        private readonly JwtIssuerOptions _issuerOptions;
        private readonly ITokenKeyStore _symmetricKeyStore;
        private readonly ITokenKeyStore _privateKeyStore;
        private readonly ILogger _logger;

        public JwtTokenIssuer(IOptions<JwtIssuerOptions> issuerOptions, IEnumerable<ITokenKeyStore> keyStores, ILoggerFactory loggerFactory)
        {
            _issuerOptions = issuerOptions.Value;

            var tokenKeyStores = keyStores.ToList();
            _symmetricKeyStore = tokenKeyStores.FirstOrDefault(s=>s.KeyType == KeyType.HSSymmetricKey);
            _privateKeyStore = tokenKeyStores.FirstOrDefault(s => s.KeyType == KeyType.RsaPrivateKey);

            _logger = loggerFactory.CreateLogger("JwtTokenIssuer");
        }

        public async Task<string> GenerateToken(string username, string password, SigType sigType = SigType.Symmetric)
        {
            _logger.LogDebug("Issuing token for {username}, {sigType}", username, sigType);

            try
            {
                var subj = TokenSubject.Create(username);
                var subjString = JsonConvert.SerializeObject(subj);
                var jti = subj.Id;

                var (keyPairId, signingCredentials) = GetSigningCredentials(sigType);
                if (signingCredentials == null)
                {
                    _logger.LogError("Failed to issue JWT token - can't retrieve signature key");
                    return null;
                }

                var user = await GetUserDetails(username);
                if (user == null)
                {
                    _logger.LogError("Failed to query user details");
                    return null;
                }

                var validPass = user.ValidatePassword(password);
                if (!validPass)
                {
                    _logger.LogError("Invalid password");
                    return null;
                }

                var now = DateTime.UtcNow;

                _logger.LogDebug("Adding token sig_id claim - {sig_id}", keyPairId);
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, subjString),
                    new Claim(JwtRegisteredClaimNames.Jti, jti),
                    new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)),
                    new Claim("keyid", sigType == SigType.Symmetric ? "symmetric_key" : keyPairId ?? "default"),
                    new Claim("roles", JsonConvert.SerializeObject(user.roles))
                };

                //var roleClaims = user.roles.Select(claim => new Claim(ClaimsIdentity.DefaultRoleClaimType, claim)).ToList();
                //claims.AddRange(roleClaims);

                var jwt = new JwtSecurityToken(
                    issuer: _issuerOptions.JwtIssuer,
                    audience: _issuerOptions.JwtAudience,
                    claims: claims,
                    notBefore: now,
                    expires: now.AddMinutes(_issuerOptions.JwtExpirationMinutes),
                    signingCredentials: signingCredentials
                );

                var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                _logger.LogDebug("Token generated {Token}", token);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to issue JWT token - {Message}", ex.ToLogMessage());
                return null;
            }
        }

        private (string keyPairId, SigningCredentials signingCredentials) GetSigningCredentials(SigType sigType)
        {
            switch (sigType)
            {
                case SigType.Asymmetric:
                {
                    if (_privateKeyStore == null)
                        return (null, null);

                    var (keyPairId, key) = _privateKeyStore.GetLatestKey(); // latest key, ordered by CreationTimeUtc
                    var rsaParameters = new OpenSSLPrivateKeyDecoder().DecodeParameters(key);
                    var securityKey = new RsaSecurityKey(rsaParameters);
                    //return (keyPairId, new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256Signature));
                    return (keyPairId, new SigningCredentials(securityKey, "RS256")); // to make it compatible with non-microsoft clients
                }
                case SigType.Symmetric:
                default:
                {
                    if (_symmetricKeyStore == null)
                        return (null, null);

                    var secret = _symmetricKeyStore.GetKey();
                    return (null, new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature));
                }
            }
        }
        
        private async Task<UserDetails> GetUserDetails(string username)
        {
            var client = new MongoClient(_issuerOptions.MongoConnectionString);
            var database = client.GetDatabase("users");
            var collection = database.GetCollection<UserDetails>("users");

            return await collection.Find(note => note.username == username).FirstOrDefaultAsync();
        }
    }
}