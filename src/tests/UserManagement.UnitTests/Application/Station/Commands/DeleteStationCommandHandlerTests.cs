using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Application.Station.Command.DeleteStation;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Station.Commands
{
    public sealed class DeleteStationCommandHandlerTests
    {
        private readonly Mock<IStationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IStationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteStationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UserManagement.Domain.Entities.Station ValidEntity(int id = 1) =>
            new() { Id = id, Code = "STA-0001", StationName = "Test Station" };

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            var command = new DeleteStationCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            var command = new DeleteStationCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var command = new DeleteStationCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete" && e.Module == "Station"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsValidationException()
        {
            var command = new DeleteStationCommand { Id = 99 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Station>(command)).Returns(ValidEntity(99));
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Failed to delete*");
        }
    }
}
