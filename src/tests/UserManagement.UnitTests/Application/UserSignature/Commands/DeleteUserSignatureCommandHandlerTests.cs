using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UserSignature.Commands
{
    public sealed class DeleteUserSignatureCommandHandlerTests
    {
        private readonly Mock<IUserSignatureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserSignatureFileStorage> _mockFileStorage = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteUserSignatureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockFileStorage.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 5);
            var existing = UserSignatureBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            _mockFileStorage
                .Setup(s => s.DeleteAsync(existing.FilePath!, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_DeletesFileFromDisk()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 5);
            var existing = UserSignatureBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            _mockFileStorage
                .Setup(s => s.DeleteAsync(existing.FilePath!, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.DeleteAsync(existing.FilePath!, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 99);
            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(99))
                .ReturnsAsync((UserManagement.Domain.Entities.UserSignature?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DeleteRepoReturnsFalse_ThrowsValidationException()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 5);
            var existing = UserSignatureBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Failed to delete*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 5);
            var existing = UserSignatureBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(5, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            _mockFileStorage
                .Setup(s => s.DeleteAsync(existing.FilePath!, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "UserSignature"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
