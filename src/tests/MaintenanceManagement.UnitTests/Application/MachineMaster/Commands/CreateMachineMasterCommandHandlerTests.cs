using AutoMapper;
using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;
using Xunit;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Commands
{
    public sealed class CreateMachineMasterCommandHandlerTests
    {
        private readonly Mock<IMachineMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMachineMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(It.IsAny<object>()))
                .Returns(MachineMasterBuilders.ValidEntity(newId));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            SetupHappyPath(5);
            var result = await CreateSut().Handle(
                MachineMasterBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(MachineMasterBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(MachineMasterBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(It.IsAny<object>()))
                .Returns(MachineMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(
                MachineMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
