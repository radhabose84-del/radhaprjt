using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Command;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.GeneratorConsumption.Commands
{
    public sealed class CreateGeneratorConsumptionCommandHandlerTests
    {
        private readonly Mock<IGeneratorConsumptionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateGeneratorConsumptionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>(It.IsAny<CreateGeneratorConsumptionCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(3);
            var command = new CreateGeneratorConsumptionCommand
            {
                GeneratorId = 1,
                OpeningEnergyReading = 100m,
                ClosingEnergyReading = 200m,
                StartTime = DateTimeOffset.UtcNow.AddHours(-2),
                EndTime = DateTimeOffset.UtcNow
            };
            var result = await CreateSut().Handle(command, CancellationToken.None);
            result.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = new CreateGeneratorConsumptionCommand
            {
                OpeningEnergyReading = 100m,
                ClosingEnergyReading = 200m,
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow
            };
            await CreateSut().Handle(command, CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = new CreateGeneratorConsumptionCommand
            {
                OpeningEnergyReading = 50m,
                ClosingEnergyReading = 150m,
                StartTime = DateTimeOffset.UtcNow.AddHours(-3),
                EndTime = DateTimeOffset.UtcNow
            };
            await CreateSut().Handle(command, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SetsEnergyFromReadings()
        {
            MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption? captured = null;
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>(It.IsAny<CreateGeneratorConsumptionCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>()))
                .Callback<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new CreateGeneratorConsumptionCommand
            {
                OpeningEnergyReading = 100m,
                ClosingEnergyReading = 300m,
                StartTime = DateTimeOffset.UtcNow.AddHours(-2),
                EndTime = DateTimeOffset.UtcNow
            }, CancellationToken.None);

            captured!.Energy.Should().Be(200m); // 300 - 100
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>(It.IsAny<CreateGeneratorConsumptionCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>()))
                .ReturnsAsync(0);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(new CreateGeneratorConsumptionCommand
            {
                OpeningEnergyReading = 100m,
                ClosingEnergyReading = 200m,
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow
            }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
