using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class PasswordLog 
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedIP { get; set; }
        public User? User { get; set; }
        
    }
}