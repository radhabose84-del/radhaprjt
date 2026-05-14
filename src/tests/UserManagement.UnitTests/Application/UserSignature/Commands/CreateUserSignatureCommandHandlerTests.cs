using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UserSignature.Commands
{
    public sealed class CreateUserSignatureCommandHandlerTests
    {
        private readonly Mock<IUserSignatureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserSignatureFileStorage> _mockFileStorage = new(MockBehavior.Strict);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateUserSignatureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockFileStorage.Object, _mockUserLookup.Object, _mockMediator.Object);

        private void SetupHappyPath(int userId, int newId = 42, string generatedFileName = "vishal-1.png")
        {
            _mockUserLookup
                .Setup(l => l.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = userId, UserName = "Vishal", FirstName = "Vishal" });

            _mockFileStorage
                .Setup(s => s.SaveAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "Vishal", userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedUserSignatureFile(
                    FileName: generatedFileName,
                    OriginalFileName: "signature.png",
                    FilePath: $"Resources\\UserManagement\\UserSignatures\\{generatedFileName}",
                    FileType: "image/png",
                    FileSize: 128));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 1);
            SetupHappyPath(userId: 1, newId: 99);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(99);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsFileStorageSaveOnce()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 1);
            SetupHappyPath(userId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.SaveAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "Vishal", 1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsGeneratedFileName()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 1);
            UserManagement.Domain.Entities.UserSignature? capturedEntity = null;
            _mockUserLookup
                .Setup(l => l.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = 1, UserName = "Vishal" });
            _mockFileStorage
                .Setup(s => s.SaveAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "Vishal", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedUserSignatureFile(
                    FileName: "vishal-1.png",
                    OriginalFileName: "signature.png",
                    FilePath: "Resources\\UserManagement\\UserSignatures\\vishal-1.png",
                    FileType: "image/png",
                    FileSize: 128));
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .Callback<UserManagement.Domain.Entities.UserSignature>(e => capturedEntity = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.FileName.Should().Be("vishal-1.png");
            capturedEntity.UserId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 1);
            SetupHappyPath(userId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "UserSignature"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FileMissing_ThrowsValidationException()
        {
            var command = new CreateUserSignatureCommand { UserId = 1, File = null };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*file is required*");
        }

        [Fact]
        public async Task Handle_UserLookupReturnsNull_ThrowsValidationException()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 999);
            _mockUserLookup
                .Setup(l => l.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserLookupDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*UserId is inactive/deleted*");
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var command = UserSignatureBuilders.ValidCreateCommand(userId: 1);
            SetupHappyPath(userId: 1, newId: 0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*creation failed*");
        }
    }
}
