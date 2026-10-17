using OpenVK.Net.Models;
using System.Text.Json;

namespace OpenVK.Net
{
    [Obsolete("This is an outdated and insecure method for obtaining an authentication token. Use `OpenVKOAuth`.", UrlFormat = "https://openvk.github.io/docs/openvk_engine/api/authorization/")]
    public class OpenVKAuth
    {
        private readonly OpenVKServer m_Server;
        private readonly OpenVKClientData? m_ClientData;

        [Obsolete("This is an outdated and insecure method for obtaining an authentication token. Use `OpenVKOAuth`.", UrlFormat = "https://openvk.github.io/docs/openvk_engine/api/authorization/")]
        public OpenVKAuth(OpenVKServer server, OpenVKClientData? clientData = null)
        {
            m_Server = server;
            m_ClientData = clientData;
        }

        [Obsolete("This is an outdated and insecure method for obtaining an authentication token. Use `OpenVKOAuth`.", UrlFormat = "https://openvk.github.io/docs/openvk_engine/api/authorization/")]
        public async Task<OpenVKTokenResponse> Authorize(string login, string password, int? twofacode = null, string? grantType = "password")
        {
            var url = $"{m_Server.GetURL()}/token?username={Uri.EscapeDataString(login)}&password={Uri.EscapeDataString(password)}&grant_type={grantType ?? "password"}";

            if (m_ClientData?.client_id != null)
                url += $"&client_id={m_ClientData.client_id}";
            else if (!string.IsNullOrEmpty(m_ClientData?.client_name))
                url += $"$client_name={m_ClientData.client_name}";

            if (twofacode != null)
                url += $"&code={twofacode}";

            var response = await m_Server.GetHttpClient().GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonResponse);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                doc.RootElement.TryGetProperty("error_reason", out var errorReason);
                doc.RootElement.TryGetProperty("error_description", out var errorDescription);

                throw new OpenVKException("An error occurred during authorization.", error.GetString(), errorReason.GetString(), errorDescription.GetString());
            }

            var tokenData = JsonSerializer.Deserialize<OpenVKTokenResponse>(jsonResponse);
            if (tokenData != null)
                return tokenData;
            throw new Exception("The token model is empty.");
        }
    }
}
