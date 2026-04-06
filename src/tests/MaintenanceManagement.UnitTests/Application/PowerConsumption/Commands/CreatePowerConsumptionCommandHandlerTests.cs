using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.PowerConsumption.Commands
{
    public sealed class CreatePowerConsumptionCommandHandlerTests
    {
        private readonly Mock<IPowerConsumptionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreatePowerConsumptionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>(It.IsAny<CreatePowerConsumptionCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.PowerConsumption());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(4);
            var result = await CreateSut().Handle(
                new CreatePowerConsumptionCommand { FeederId = 1, FeederTypeId = 1, OpeningReading = 100m, ClosingReading = 200m },
                CancellationToken.None);
            result.Should().Be(4);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new CreatePowerConsumptionCommand { FeederId = 1, FeederTypeId = 1, OpeningReading = 10m, ClosingReading = 20m },
                CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new CreatePowerConsumptionCommand { FeederId = 1, FeederTypeId = 1, OpeningReading = 10m, ClosingReading = 30m },
                CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SetsTotalUnitsAsKwConversion()
        {
            MaintenanceManagement.Domain.Entities.Power.PowerConsumption? captured = null;
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>(It.IsAny<CreatePowerConsumptionCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.PowerConsumption());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>()))
                .Callback<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new CreatePowerConsumptionCommand { FeederId = 1, FeederTypeId = 1, OpeningReading = 5m, ClosingReading = 10m },
                CancellationToken.None);

            captured!.TotalUnits.Should().Be(5000m); // (10 - 5) * 1000
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>(It.IsAny<CreatePowerConsumptionCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.PowerConsumption());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>()))
                .ReturnsAsync(0);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(
                new CreatePowerConsumptionCommand { OpeningReading = 1m, ClosingReading = 5m },
                CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
