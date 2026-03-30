using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Commands.UpdateModule;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Modules.Commands
{
    public sealed class UpdateModuleCommandHandlerTests
    {
        private readonly Mock<IModuleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IModuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateModuleCommandHandler>> _mockLogger = new();

        private UpdateModuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            // Arrange
            var command = new UpdateModuleCommand { ModuleId = 1, ModuleName = "UpdatedModule" };
            var existingModule = new UserManagement.Domain.Entities.Modules
            {
                Id = 1,
                ModuleName = "OldModule",
                IsDeleted = false
            };
            var mappedModule = new UserManagement.Domain.Entities.Modules { Id = 1, ModuleName = "UpdatedModule" };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(1))
                .ReturnsAsync(existingModule);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Modules>(command))
                .Returns(mappedModule);

            _mockCommandRepo
                .Setup(r => r.UpdateModuleAsync(mappedModule))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ModuleNotFound_ThrowsValidationException()
        {
            // Arrange
            var command = new UpdateModuleCommand { ModuleId = 999, ModuleName = "NoSuch" };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Modules?)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DeletedModule_ThrowsValidationException()
        {
            // Arrange
            var command = new UpdateModuleCommand { ModuleId = 3, ModuleName = "Module" };
            var deletedModule = new UserManagement.Domain.Entities.Modules
            {
                Id = 3,
                ModuleName = "DeletedModule",
                IsDeleted = true
            };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(3))
                .ReturnsAsync(deletedModule);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*deleted*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = new UpdateModuleCommand { ModuleId = 1, ModuleName = "NewName" };
            var existingModule = new UserManagement.Domain.Entities.Modules
            {
                Id = 1,
                ModuleName = "OldName",
                IsDeleted = false
            };
            var mappedModule = new UserManagement.Domain.Entities.Modules { Id = 1, ModuleName = "NewName" };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(1))
                .ReturnsAsync(existingModule);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Modules>(command))
                .Returns(mappedModule);

            _mockCommandRepo
                .Setup(r => r.UpdateModuleAsync(mappedModule))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
