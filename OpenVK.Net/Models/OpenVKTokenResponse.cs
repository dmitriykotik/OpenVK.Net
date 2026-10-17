namespace OpenVK.Net.Models
{
    public class OpenVKTokenResponse
    {
        public required string access_token { get; set; }
        public int expires_in { get; set; }
        public long user_id { get; set; }
    }
}
