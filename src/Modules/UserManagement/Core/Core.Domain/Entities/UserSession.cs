namespace Core.Domain.Entities
{
    public class UserSessions
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? JwtId  { get; set; }     
        public string? BrowserInfo { get; set; }   
        public DateTime CreatedAt  { get; set; }
        public DateTime ExpiresAt { get; set; }
        public byte IsActive { get; set; }        
        public DateTime LastActivity { get; set; }     
    }

}