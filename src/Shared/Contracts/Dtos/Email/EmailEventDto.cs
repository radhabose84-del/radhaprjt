namespace Contracts.Dtos.Email
{
    public class EmailEventDto
    {
        public string ToEmail { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public string provider { get; set; }="Gmail";
    }
}