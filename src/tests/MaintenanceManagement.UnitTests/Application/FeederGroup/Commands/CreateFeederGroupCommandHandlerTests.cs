using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup;

namespace MaintenanceManagement.UnitTests.Application.FeederGroup.Commands
{
    public sealed class CreateFeederGroupCommandHandlerTests
    {
        private readonly Mock<IFeederGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateFeederGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(It.IsAny<CreateFeederGroupCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.FeederGroup());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.FeederGroup>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(7);
            var result = await CreateSut().Handle(new CreateFeederGroupCommand(), CancellationToken.None);
            result.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateFeederGroupCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.FeederGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(It.IsAny<CreateFeederGroupCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.FeederGroup());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.Power.FeederGroup>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(new CreateFeederGroupCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
