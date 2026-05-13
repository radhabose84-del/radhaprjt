using AutoMapper;
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
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteUserSignatureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsValidationException()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Failed to delete*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "UserSignature"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FailedDelete_DoesNotPublishAuditEvent()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(false);

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
