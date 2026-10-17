using System.Text.Json;

namespace OpenVK.Net
{
    public class OpenVKClient
    {
        private readonly OpenVKServer m_Server;
        private readonly string m_AuthToken;

        public OpenVKClient(OpenVKServer server, string authToken)
        {
            m_Server = server;
            m_AuthToken = authToken;
        }

        public async Task<string> CallAsync(string methodName, Dictionary<string, string>? parameters = null)
        {
            parameters ??= [];
            
            parameters["access_token"] = m_AuthToken;

            var url = $"{m_Server.GetURL()}/method/{methodName}";

            var content = new FormUrlEncodedContent(parameters);
            var response = await m_Server.GetHttpClient().PostAsync(url, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonResponse);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                doc.RootElement.TryGetProperty("error_reason", out var errorReason);
                doc.RootElement.TryGetProperty("error_description", out var errorDescription);

                throw new OpenVKException("An error occurred while executing your request.", error.GetString(), errorReason.GetString(), errorDescription.GetString());
            }

            return jsonResponse;
        }
    }
}
