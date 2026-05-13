using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class UserSignature : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public byte[]? SignatureImage { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public int FileSizeBytes { get; set; }

        public User User { get; set; } = null!;
    }
}
