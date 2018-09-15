using System;
using System.Linq;
using System.Threading.Tasks;
using afs.jwt.abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace afs.jwt.example
{
    [ApiVersion("1")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class Controller : ControllerBase
    {
        private readonly ITokenIssuer _tokenIssuer;
        private readonly ITokenValidator _tokenValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public Controller(ITokenIssuer tokenIssuer, ITokenValidator tokenValidator, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
        {
            _tokenIssuer = tokenIssuer;
            _tokenValidator = tokenValidator;
            _httpContextAccessor = httpContextAccessor;
            _logger = loggerFactory.CreateLogger("Controller");
        }

        [HttpGet("token")]
        public async Task<string> GetToken([FromQuery] string sigType = "symmetric")
        {
            var (username, password) = _httpContextAccessor.HttpContext.Request.GetApiKeyCredentials();
            var token = await _tokenIssuer.GenerateToken(username, password, sigType == "symmetric" ? SigType.Symmetric : SigType.Asymmetric);
            return token;
        }
    }

    // admin:Qwerty!23 - YWRtaW46UXdlcnR5ITIz
    // bruce:Qwerty!23 - YnJ1Y2U6UXdlcnR5ITIz
    // kerkoroff:Qwerty!23 - a2Vya29yb2ZmOlF3ZXJ0eSEyMw==
    public static class AuthenticationExtensions
    {
        public static string GetRequestApiKey(this HttpRequest request)
        {
            request.Headers.TryGetValue("Authorization", out var autorizationHeader);

            var token = autorizationHeader.FirstOrDefault();
            return token != null && token.StartsWith("Basic", StringComparison.OrdinalIgnoreCase)
                ? token.Substring("Basic".Length).Trim()
                : token;
        }

        public static (string username, string password) GetApiKeyCredentials(this string apiKey)
        {
            var decodedBytes = Convert.FromBase64String(apiKey);
            var decodedTxt = System.Text.Encoding.UTF8.GetString(decodedBytes);
            var parts = decodedTxt.Split(':');
            return (parts[0], parts[1]);
        }

        public static (string username, string password) GetApiKeyCredentials(this HttpRequest request)
            => request.GetRequestApiKey().GetApiKeyCredentials();
    }
}