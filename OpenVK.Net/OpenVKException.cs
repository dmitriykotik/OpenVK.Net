namespace OpenVK.Net
{
    public class OpenVKException : Exception
    {
        public string? Error { get; } = null;
        public string? ErrorReason { get; } = null;
        public string? ErrorDescription { get; } = null;

        public OpenVKException(string message, string? error = null, string? errorReason = null, string? errorDescription = null) : base(message)
        {
            Error = error;
            ErrorReason = errorReason;
            ErrorDescription = errorDescription;
        }
    }
}
