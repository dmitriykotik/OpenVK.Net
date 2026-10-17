namespace OpenVK.Net.Models
{
    public class OpenVKErrorResponse
    {
        public required string error { get; set; }
        public string? error_reason { get; set; }
        public string? error_description { get; set; }
    }
}
