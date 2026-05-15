using Microsoft.AspNetCore.Http;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class UserSignatureBuilders
    {
        public static IFormFile BuildFormFile(
            string fileName = "signature.png",
            string contentType = "image/png",
            int sizeBytes = 128)
        {
            var bytes = new byte[sizeBytes];
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        public static CreateUserSignatureCommand ValidCreateCommand(
            int userId = 1,
            IFormFile? file = null) =>
            new()
            {
                UserId = userId,
                File = file ?? BuildFormFile()
            };

        public static UpdateUserSignatureCommand ValidUpdateCommand(
            int id = 1,
            IFormFile? file = null,
            Status isActive = Status.Active) =>
            new()
            {
                Id = id,
                File = file ?? BuildFormFile(),
                IsActive = isActive
            };

        public static DeleteUserSignatureCommand ValidDeleteCommand(int id = 1) =>
            new() { Id = id };

        public static UserManagement.Domain.Entities.UserSignature ValidEntity(int id = 1, int userId = 1) =>
            new()
            {
                Id = id,
                UserId = userId,
                FileName = $"vishal-{userId}.png",
                OriginalFileName = "signature.png",
                FilePath = $"Resources\\UserManagement\\UserSignatures\\vishal-{userId}.png",
                FileType = "image/png",
                FileSize = 128,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static UserManagement.Domain.Entities.UserSignature ValidEntityWithUser(int id = 1, int userId = 1) =>
            new()
            {
                Id = id,
                UserId = userId,
                FileName = $"vishal-{userId}.png",
                OriginalFileName = "signature.png",
                FilePath = $"Resources\\UserManagement\\UserSignatures\\vishal-{userId}.png",
                FileType = "image/png",
                FileSize = 128,
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
