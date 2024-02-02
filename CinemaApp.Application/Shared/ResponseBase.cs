using System.Text.Json.Serialization;

namespace CinemaApp.Application.Shared
{
    public class ResponseBase
    {
        public bool Success { get; }
        public string? Title { get; }
        public string? Details { get; }
        
        [JsonIgnore]
        public bool RequestValid { get; }

        public ResponseBase(bool success, string? title, string? details, bool requestValid)
        {
            Success = success;
            Title = title;
            Details = details;
            RequestValid = requestValid;
        }
    }
}
