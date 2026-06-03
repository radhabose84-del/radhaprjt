using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Application.Location.Command.DeleteLocation;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Location.Commands
{
    public sealed class DeleteLocationCommandHandlerTests
    {
        private readonly Mock<ILocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UserManagement.Domain.Entities.Location ValidEntity(int id = 1) =>
            new() { Id = id, Code = "LOC-0001", LocationName = "Test Location" };

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            var command = new DeleteLocationCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            var command = new DeleteLocationCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Location>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var command = new DeleteLocationCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(ValidEntity());
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete" && e.Module == "Location"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsValidationException()
        {
            var command = new DeleteLocationCommand { Id = 99 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Location>(command)).Returns(ValidEntity(99));
            _mockCommandRepo.Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Location>())).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Failed to delete*");
        }
    }
}
