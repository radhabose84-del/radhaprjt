using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.UserSignature.Queries.GetUserSignatureById
{
    public class UserSignatureByIdDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmailId { get; set; }
        public byte[]? SignatureImage { get; set; }
        public string? SignatureBase64 { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public int FileSizeBytes { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
