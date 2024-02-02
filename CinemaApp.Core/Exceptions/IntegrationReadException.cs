namespace CinemaApp.Core.Exceptions
{
    public class IntegrationReadException : Exception
    {
        public IntegrationReadException(string message)
        {
            Message = message;
        }

        public string? Message { get; set; }
        public int? StatusCode { get; set; }
    }
}
