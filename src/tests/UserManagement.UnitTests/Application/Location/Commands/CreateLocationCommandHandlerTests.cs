using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Application.Location.Command.CreateLocation;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Location.Commands
{
    public sealed class CreateLocationCommandHandlerTests
    {
        private readonly Mock<ILocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static CreateLocationCommand ValidCommand() =>
            new()
            {
                Code = "LOC-0001",
                LocationName = "Main Warehouse",
                Description = "Primary location"
            };

        private static UserManagement.Domain.Entities.Location ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                Code = "LOC-0001",
                LocationName = "Main Warehouse"
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = ValidCommand();
            var entity = ValidEntity(1);

            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = ValidCommand();
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Location>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = ValidCommand();
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.Module == "Location"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var command = ValidCommand();
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*creation failed*");
        }
    }
}
