namespace PartyManagement.Application.GST.DTOs
{
    public class AuthData
    {
        public string? ClientId { get; set; }
        public string? UserName { get; set; }
        public string? AuthToken { get; set; }
        public string? Sek { get; set; }
        public string? TokenExpiry { get; set; }
    }
}