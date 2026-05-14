using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class UserSignature : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? FileName { get; set; }           // generated, e.g. vishal-2414.png
        public string? OriginalFileName { get; set; }   // as uploaded by user, e.g. output-onlinepngtools.png
        public string? FilePath { get; set; }           // Resources\UserManagement\UserSignatures\vishal-2414.png
        public string? FileType { get; set; }           // image/png
        public long FileSize { get; set; }              // bytes

        public User User { get; set; } = null!;
    }
}
