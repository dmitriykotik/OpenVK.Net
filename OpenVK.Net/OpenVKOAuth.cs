using OpenVK.Net.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace OpenVK.Net
{
    public class OpenVKOAuth
    {
        private readonly OpenVKServer m_Server;
        private readonly int m_Port;
        private readonly string m_RedirectUrl;

        private string? m_AuthUrl;

        public OpenVKOAuth(OpenVKServer server, int? localServerPort = null)
        {
            m_Server = server;
            m_Port = localServerPort ?? GetAvailablePort();
            m_RedirectUrl = $"http://localhost:{m_Port}/callback/";
        }

        public string GenOAuthLink(OpenVKClientData clientData, bool revoke = false, string responseType = "php")
        {
            if (clientData.client_id == null && string.IsNullOrEmpty(clientData.client_name))
                throw new ArgumentNullException(nameof(clientData), "client_name (or client_id) params are required.");

            var queryParams = HttpUtility.ParseQueryString(string.Empty);

            if (clientData.client_id != null)
                queryParams["client_id"] = clientData.client_id.ToString();
            else
                queryParams["client_name"] = clientData.client_name;

            queryParams["redirect_uri"] = m_RedirectUrl;
            queryParams["display"] = "page";
            queryParams["response_type"] = responseType;
            queryParams["revoke"] = revoke ? "1" : "0";

            m_AuthUrl = $"{m_Server.GetURL().TrimEnd('/')}/authorize?{queryParams}";
            return m_AuthUrl;
        }

        public async Task<OpenVKTokenResponse> StartLocalAuthServer(CancellationToken cancellationToken = default)
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add(m_RedirectUrl);
            listener.Start();

            using var reg = cancellationToken.Register(() =>
            {
                try { listener.Stop(); } catch { }
            });

            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync();
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("Authorization has been revoked.", cancellationToken);
            }

            var request = context.Request;

            string? token = request.QueryString["access_token"];
            string? error = request.QueryString["error"];
            string? errorReason = request.QueryString["error_reason"];
            string? errorDescription = request.QueryString["error_description"];

            using (var response = context.Response)
            {
                response.ContentType = "text/html; charset=utf-8";
                byte[] buffer = Encoding.UTF8.GetBytes(GenHTML(!string.IsNullOrEmpty(error), error, errorReason, errorDescription));
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, cancellationToken);
            }

            listener.Stop();

            if (!string.IsNullOrEmpty(error))
                throw new OpenVKException($"An error occurred during authorization.", error, errorReason, errorDescription);

            if (string.IsNullOrEmpty(token))
                throw new Exception("The token field is empty.");

            int.TryParse(request.QueryString["expires_in"], out int expiresIn);
            long.TryParse(request.QueryString["user_id"], out long userId);

            return new OpenVKTokenResponse
            {
                access_token = token,
                expires_in = expiresIn,
                user_id = userId
            };
        }

        private static int GetAvailablePort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }

        private static string GenHTML(bool isError, string? error, string? errorReason, string? errorDescription)
        {
            if (isError)
            {
                string safeError = HttpUtility.HtmlEncode(error ?? "undefined");
                string safeReason = HttpUtility.HtmlEncode(errorReason ?? "undefined");
                string safeDesc = HttpUtility.HtmlEncode(errorDescription ?? "undefined");

                return $"<html><body style='font-family:sans-serif;text-align:center;padding-top:50px;'><h1>Error!</h1><p>Error: {safeError}</p><p>Error reason: {safeReason}</p><p>Error description: {safeDesc}</p></body></html>";
            }

            return @"<html><body style='font-family:sans-serif;text-align:center;padding-top:50px;'><h1>Successfully!</h1><p>You can now close this page.</p></body></html>";
        }
    }
}