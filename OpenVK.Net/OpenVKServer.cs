using OpenVK.Net.Models;
using System.Net.NetworkInformation;

namespace OpenVK.Net
{
    public class OpenVKServer
    {
        private readonly Uri m_CleanDomain;
        private readonly HttpClient m_HttpClient;

        private readonly int m_DefaultPingTimeout;

        public OpenVKServer(string domain, HttpClient? httpClient = null, OpenVKServerSettings? serverSettings = null)
        {
            const int constDefaultPingResponse = 3000;
            bool m_PingServerBeforeConnecting;

            m_CleanDomain = new Uri(domain);
            m_HttpClient = httpClient ?? new HttpClient();

            m_PingServerBeforeConnecting = serverSettings?.PingServerBeforeConnecting ?? false;
            m_DefaultPingTimeout = serverSettings?.DefaultPingTimeout ?? constDefaultPingResponse;

            if (m_PingServerBeforeConnecting)
            {
                if (!Ping())
                    throw new TimeoutException($"Failed to ping server `{GetURL()}`.");
            }
        }

        public string GetURLHost() => m_CleanDomain.Host;
        public string GetURL() => m_CleanDomain.GetLeftPart(UriPartial.Authority);
        public HttpClient GetHttpClient() => m_HttpClient;

        public bool Ping(int? timeout = null)
        {
            var _timeout = timeout ?? m_DefaultPingTimeout;
            using var ping = new Ping();
            var pingReply = ping.Send(m_CleanDomain.Host, _timeout);

            var result = pingReply.Status == IPStatus.Success;
            return result;
        }

        public async Task<bool> PingAsync(int? timeout = null)
        {
            var _timeout = timeout ?? m_DefaultPingTimeout;
            using var ping = new Ping();
            var pingReply = await ping.SendPingAsync(m_CleanDomain.Host, _timeout);

            var result = pingReply.Status == IPStatus.Success;
            return result;
        }
    }
}
