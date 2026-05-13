using AutoMapper;
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
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateUserSignatureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsOne()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ReturnsZero()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = UserSignatureBuilders.ValidUpdateCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
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
