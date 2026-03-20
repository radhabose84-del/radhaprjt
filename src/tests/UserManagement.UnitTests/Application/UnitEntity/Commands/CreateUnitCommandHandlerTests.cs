using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;
using DomainUnit = UserManagement.Domain.Entities.Unit;

namespace UserManagement.UnitTests.Application.UnitEntity.Commands
{
    public sealed class CreateUnitCommandHandlerTests
    {
        private readonly Mock<IUnitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CreateUnitCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CreateUnitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object);

        private void SetupHappyPath(CreateUnitCommand command, DomainUnit entity, int newId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.UnitName!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<DomainUnit>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateUnitAsync(It.IsAny<DomainUnit>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();
            var entity = UnitEntityBuilders.ValidEntity();
            SetupHappyPath(command, entity, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();
            var entity = UnitEntityBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateUnitAsync(It.IsAny<DomainUnit>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();
            var entity = UnitEntityBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Unit"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateUnitName_ThrowsValidationException()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.UnitName!))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();
            var entity = UnitEntityBuilders.ValidEntity(id: 0);

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.UnitName!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<DomainUnit>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateUnitAsync(It.IsAny<DomainUnit>()))
                .ReturnsAsync(0);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not created*");
        }
    }
}
