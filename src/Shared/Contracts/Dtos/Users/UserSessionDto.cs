using System;

namespace Contracts.Dtos.Users
{
    public class UserSessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string JwtId { get; set; } = string.Empty;
        public string BrowserInfo { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public int IsActive { get; set; }
        public DateTimeOffset LastActivity { get; set; }
    }
}