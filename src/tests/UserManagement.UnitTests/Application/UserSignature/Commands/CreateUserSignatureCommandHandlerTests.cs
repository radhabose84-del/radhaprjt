using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UserSignature.Commands
{
    public sealed class CreateUserSignatureCommandHandlerTests
    {
        private readonly Mock<IUserSignatureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateUserSignatureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = UserSignatureBuilders.ValidCreateCommand();
            var entity = UserSignatureBuilders.ValidEntity(1);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = UserSignatureBuilders.ValidCreateCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserSignature>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = UserSignatureBuilders.ValidCreateCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(1);

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
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var command = UserSignatureBuilders.ValidCreateCommand();
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserSignature>(command))
                .Returns(UserSignatureBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserSignature>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*creation failed*");
        }
    }
}
