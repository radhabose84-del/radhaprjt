using AutoMapper;
using Contracts.Common;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.PartyGroup.Commands
{
    public sealed class UpdatePartyGroupCommandHandlerTests
    {
        private readonly Mock<IPartyGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPartyGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdatePartyGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(UpdatePartyGroupCommand command)
        {
            var entity = PartyGroupBuilders.ValidEntity(command.Id);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockQueryRepo
                .Setup(r => r.IsPartyGroupLinkedAsync(command.Id))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = PartyGroupBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = PartyGroupBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(command.Id, It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = PartyGroupBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsExceptionRules()
        {
            var command = PartyGroupBuilders.ValidUpdateCommand(isActive: 0);

            _mockQueryRepo
                .Setup(r => r.IsPartyGroupLinkedAsync(command.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () =>
                await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked*");
        }
    }
}
