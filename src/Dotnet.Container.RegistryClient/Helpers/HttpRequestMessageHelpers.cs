using System.Net.Http;
using System.Text;

namespace System.Net
{
    public static class HttpRequestMessageHelpers
    {
        public static void AddBasicAuthorizationHeader(this HttpRequestMessage message, string username, string password)
        {
            if (username != null && password != null)
            {
                // According to RFC 2617, use ISO-8859-1 and *NOT* UTF-8
                var encodedUsernamePassword = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"));
                message.Headers.Add("Authorization", $"Basic {encodedUsernamePassword}");
            }
        }

    }
}
