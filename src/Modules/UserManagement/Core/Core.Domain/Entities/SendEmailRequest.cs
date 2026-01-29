namespace Core.Domain.Entities
{
    public class SendEmailRequest
    {
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}