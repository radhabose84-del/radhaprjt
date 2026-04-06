using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder;

namespace MaintenanceManagement.UnitTests.Application.Feeder.Commands
{
    public sealed class CreateFeederCommandHandlerTests
    {
        private readonly Mock<IFeederCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateFeederCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(It.IsAny<CreateFeederCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.Feeder());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);
            var result = await CreateSut().Handle(new CreateFeederCommand(), CancellationToken.None);
            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateFeederCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(It.IsAny<CreateFeederCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.Feeder());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(new CreateFeederCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
