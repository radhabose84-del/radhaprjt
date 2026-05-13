using MediatR;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.UserSignature.Command.UpdateUserSignature
{
    public class UpdateUserSignatureCommand : IRequest<int>
    {
        public int Id { get; set; }
        public byte[]? SignatureImage { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public int FileSizeBytes { get; set; }
        public Status IsActive { get; set; }
    }
}
