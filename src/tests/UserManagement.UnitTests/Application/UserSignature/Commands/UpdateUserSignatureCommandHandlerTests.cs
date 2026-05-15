using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UserSignature.Commands
{
    public sealed class UpdateUserSignatureCommandHandlerTests
    {
        private readonly Mock<IUserSignatureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserSignatureFileStorage> _mockFileStorage = new(MockBehavior.Strict);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateUserSignatureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockFileStorage.Object, _mockUserLookup.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_WithNewFile_ReturnsOne()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(id: 5);
            var existing = UserSignatureBuilders.ValidEntity(id: 5, userId: 10);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockUserLookup
                .Setup(l => l.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = 10, UserName = "Vishal" });

            _mockFileStorage
                .Setup(s => s.SaveAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "Vishal", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedUserSignatureFile(
                    FileName: "vishal-10.png",
                    OriginalFileName: "signature.png",
                    FilePath: "Resources\\UserManagement\\UserSignatures\\vishal-10.png",
                    FileType: "image/png",
                    FileSize: 128));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_NoFile_SkipsFileStorageSave()
        {
            // File omitted → only IsActive toggled
            var command = new UpdateUserSignatureCommand { Id = 5, File = null, IsActive = UserManagement.Domain.Enums.Common.Enums.Status.Inactive };
            var existing = UserSignatureBuilders.ValidEntity(id: 5, userId: 10);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
            _mockFileStorage.Verify(
                s => s.SaveAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ReturnsZero()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(id: 5);
            var existing = UserSignatureBuilders.ValidEntity(id: 5, userId: 10);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockUserLookup
                .Setup(l => l.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = 10, UserName = "Vishal" });

            _mockFileStorage
                .Setup(s => s.SaveAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "Vishal", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedUserSignatureFile(
                    FileName: "vishal-10.png",
                    OriginalFileName: "signature.png",
                    FilePath: "Resources\\UserManagement\\UserSignatures\\vishal-10.png",
                    FileType: "image/png",
                    FileSize: 128));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(99))
                .ReturnsAsync((UserManagement.Domain.Entities.UserSignature?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand(id: 5);
            var existing = UserSignatureBuilders.ValidEntity(id: 5, userId: 10);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockUserLookup
                .Setup(l => l.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = 10, UserName = "Vishal" });

            _mockFileStorage
                .Setup(s => s.SaveAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "Vishal", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedUserSignatureFile(
                    FileName: "vishal-10.png",
                    OriginalFileName: "signature.png",
                    FilePath: "Resources\\UserManagement\\UserSignatures\\vishal-10.png",
                    FileType: "image/png",
                    FileSize: 128));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "UserSignature"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
