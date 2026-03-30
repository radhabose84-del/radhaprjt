using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Commands.CreateModule;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Modules.Commands
{
    public sealed class CreateModuleCommandHandlerTests
    {
        private readonly Mock<IModuleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateModuleCommandHandler>> _mockLogger = new();

        private CreateModuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, null!, _mockMediator.Object, _mockLogger.Object);

        private void SetupHappyPath()
        {
            _mockCommandRepo
                .Setup(r => r.AddModuleAsync(It.IsAny<UserManagement.Domain.Entities.Modules>()))
                .Returns(Task.CompletedTask);

            _mockCommandRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsModuleId()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateModuleCommand { ModuleName = "TestModule" };
            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(0); // default int since no DB; entity.Id is 0 when not persisted
        }

        [Fact]
        public async Task Handle_NullModuleName_ThrowsValidationException()
        {
            // Arrange
            var command = new CreateModuleCommand { ModuleName = null };
            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*required*");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddModuleOnce()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateModuleCommand { ModuleName = "Inventory" };
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.AddModuleAsync(It.IsAny<UserManagement.Domain.Entities.Modules>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            SetupHappyPath();
            var command = new CreateModuleCommand { ModuleName = "Sales" };
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Modules"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
