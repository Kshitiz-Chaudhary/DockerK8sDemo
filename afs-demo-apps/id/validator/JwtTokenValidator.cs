using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using afs.jwt.abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenSSL.PublicKeyDecoder;

namespace afs.jwt.validator
{
    public class JwtTokenValidator : ITokenValidator
    {
        private static readonly TimeSpan _clockSkew = TimeSpan.FromSeconds(10);

        private readonly JwtIssuerOptions _issuerOptions;
        private readonly ITokenKeyStore _symmetricKeyStore;
        private readonly ITokenKeyStore _publicKeyStore;
        private readonly ILogger _logger;

        public JwtTokenValidator(IOptions<JwtIssuerOptions> issuerOptions, IEnumerable<ITokenKeyStore> keyStores, ILoggerFactory loggerFactory)
        {
            _issuerOptions = issuerOptions.Value;

            var tokenKeyStores = keyStores.ToList();
            _symmetricKeyStore = tokenKeyStores.FirstOrDefault(s => s.KeyType == KeyType.HSSymmetricKey);
            _publicKeyStore = tokenKeyStores.FirstOrDefault(s => s.KeyType == KeyType.RsaPublicKey);

            _logger = loggerFactory.CreateLogger("JwtTokenValidator");
        }

        public bool IsTokenValid(string tokenString)
            => GetToken(tokenString) != null;

        public string GetTokenJti(string tokenString)
            => GetToken(tokenString)?.Id;

        private JwtSecurityToken GetToken(string tokenString)
        {
            if (String.IsNullOrEmpty(tokenString))
            {
                _logger.LogDebug("Invalid security token - token string is empty");
                return null;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();

                // read signature type from tocken header
                var token = handler.ReadJwtToken(tokenString);
                if (token == null)
                {
                    _logger.LogDebug("Invalid security token - can't read from token string");
                    return null;
                }

                var keyPairId = token.Claims.FirstOrDefault(c => c.Type == "sig_id")?.Value;
                var issuerSigningKey = GetValidationKey(token.SignatureAlgorithm, keyPairId);
                if (issuerSigningKey == null)
                    return null;

                var validatonParams = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = issuerSigningKey,
                    ValidateIssuer = true,
                    ValidIssuer = _issuerOptions.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _issuerOptions.JwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = _clockSkew,
                    RequireExpirationTime = true
                };

                handler.ValidateToken(tokenString, validatonParams, out var secToken);
                return secToken as JwtSecurityToken;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to validate token - {Message}", ex.ToLogMessage());
                return null;
            }
        }

        private SecurityKey GetValidationKey(string signatureAlgorithm, string keyPairId = null)
        {
            try
            {
                // "alg": "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256
                switch (signatureAlgorithm)
                {
                    case SecurityAlgorithms.RsaSha256:
                    case SecurityAlgorithms.RsaSha384:
                    case SecurityAlgorithms.RsaSha512:
                    case SecurityAlgorithms.RsaSha256Signature:
                    case SecurityAlgorithms.RsaSha384Signature:
                    case SecurityAlgorithms.RsaSha512Signature:
                    case SecurityAlgorithms.RsaOAEP:
                    case SecurityAlgorithms.RsaPKCS1:
                    case SecurityAlgorithms.RsaOaepKeyWrap:
                        return GetAsymmetricSecurityKey(keyPairId);
                    default:
                        return GetSymmetricSecurityKey();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate security key", ex.ToLogMessage());
                return null;
            }
        }

        private SecurityKey GetAsymmetricSecurityKey(string keyPairId)
        {
            if (_publicKeyStore == null)
            {
                _logger.LogError("Public Key store not available");
                return null;
            }

            // get key by keyPairId or latest
            var key = _publicKeyStore.GetKey(keyPairId);
            if (key == null)
            {
                _logger.LogError("Couldn't retrieve public key");
                return null;
            }

            var securityKey = new RsaSecurityKey(new OpenSSLPublicKeyDecoder().DecodeParameters(key));

            _logger.LogError("Created security key {SecurityKeyId}", securityKey.KeyId);
            return securityKey;
        }

        private SecurityKey GetSymmetricSecurityKey()
        {
            if (_symmetricKeyStore == null)
            {
                _logger.LogError("Symmetric Key store not available");
                return null;
            }

            var key = _symmetricKeyStore.GetKey();
            if (key == null)
            {
                _logger.LogError("Couldn't retrieve symmetric signing key");
                return null;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

            _logger.LogError("Created security key {SecurityKeyId}", securityKey.KeyId);
            return securityKey;
        }
    }
}