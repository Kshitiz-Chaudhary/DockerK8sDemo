/*using System;

namespace afs.jwt.abstractions
{
    public static class AuthenticationExtensions
    {
        public static string GetRequestToken(this HttpRequest request)
        {
            request.Headers.TryGetValue("Authorization", out var autorizationHeader);

            var token = autorizationHeader.FirstOrDefault();
            return token != null && token.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase)
                ? token.Substring("Bearer".Length).Trim()
                : token;
        }
    }
}*/