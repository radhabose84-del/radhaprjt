using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class UserSignatureBuilders
    {
        public static byte[] SmallPngBytes() =>
            new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D
            };

        public static CreateUserSignatureCommand ValidCreateCommand(
            int userId = 1,
            string fileName = "signature.png",
            string contentType = "image/png",
            byte[]? bytes = null) =>
            new()
            {
                UserId = userId,
                FileName = fileName,
                ContentType = contentType,
                SignatureImage = bytes ?? SmallPngBytes(),
                FileSizeBytes = (bytes ?? SmallPngBytes()).Length
            };

        public static UpdateUserSignatureCommand ValidUpdateCommand(
            int id = 1,
            string fileName = "updated_signature.png",
            string contentType = "image/png",
            Status isActive = Status.Active,
            byte[]? bytes = null) =>
            new()
            {
                Id = id,
                FileName = fileName,
                ContentType = contentType,
                SignatureImage = bytes ?? SmallPngBytes(),
                FileSizeBytes = (bytes ?? SmallPngBytes()).Length,
                IsActive = isActive
            };

        public static DeleteUserSignatureCommand ValidDeleteCommand(int id = 1) =>
            new() { Id = id };

        public static UserManagement.Domain.Entities.UserSignature ValidEntity(int id = 1, int userId = 1) =>
            new()
            {
                Id = id,
                UserId = userId,
                SignatureImage = SmallPngBytes(),
                FileName = "signature.png",
                ContentType = "image/png",
                FileSizeBytes = SmallPngBytes().Length,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static UserManagement.Domain.Entities.UserSignature ValidEntityWithUser(int id = 1, int userId = 1) =>
            new()
            {
                Id = id,
                UserId = userId,
                SignatureImage = SmallPngBytes(),
                FileName = "signature.png",
                ContentType = "image/png",
                FileSizeBytes = SmallPngBytes().Length,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                User = new UserManagement.Domain.Entities.User
                {
                    UserId = userId,
                    FirstName = "Test",
                    LastName = "User",
                    EmailId = "test.user@example.com"
                }
            };
    }
}
