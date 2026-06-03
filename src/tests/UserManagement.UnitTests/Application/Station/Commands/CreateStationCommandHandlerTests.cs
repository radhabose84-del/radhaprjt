using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Application.Station.Command.CreateStation;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Station.Commands
{
    public sealed class CreateStationCommandHandlerTests
    {
        private readonly Mock<IStationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateStationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static CreateStationCommand ValidCommand() =>
            new()
            {
                Code = "STA-0001",
                StationName = "Central Station",
                Description = "Primary station"
            };

        private static UserManagement.Domain.Entities.Station ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                Code = "STA-0001",
                StationName = "Central Station"
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = ValidCommand();
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity(1));
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = ValidCommand();
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Station>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = ValidCommand();
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.Module == "Station"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var command = ValidCommand();
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*creation failed*");
        }
    }
}
