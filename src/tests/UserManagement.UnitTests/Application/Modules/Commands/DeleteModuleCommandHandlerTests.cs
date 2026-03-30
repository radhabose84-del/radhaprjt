using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Commands.DeleteModule;

namespace UserManagement.UnitTests.Application.Modules.Commands
{
    public sealed class DeleteModuleCommandHandlerTests
    {
        private readonly Mock<IModuleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IModuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteModuleCommandHandler>> _mockLogger = new();

        private DeleteModuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            // Arrange
            var command = new DeleteModuleCommand { ModuleId = 1 };
            var existingModule = new UserManagement.Domain.Entities.Modules
            {
                Id = 1,
                ModuleName = "TestModule",
                IsDeleted = false
            };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(1))
                .ReturnsAsync(existingModule);

            _mockCommandRepo
                .Setup(r => r.DeleteModuleAsync(1))
                .Returns(Task.CompletedTask);

            _mockCommandRepo
                .Setup(r => r.SaveChangesAsync())
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
            var command = new DeleteModuleCommand { ModuleId = 999 };

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
        public async Task Handle_AlreadyDeletedModule_ThrowsValidationException()
        {
            // Arrange
            var command = new DeleteModuleCommand { ModuleId = 2 };
            var deletedModule = new UserManagement.Domain.Entities.Modules
            {
                Id = 2,
                ModuleName = "DeletedModule",
                IsDeleted = true
            };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(2))
                .ReturnsAsync(deletedModule);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*deleted*");
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteModuleOnce()
        {
            // Arrange
            var command = new DeleteModuleCommand { ModuleId = 5 };
            var existingModule = new UserManagement.Domain.Entities.Modules
            {
                Id = 5,
                ModuleName = "Finance",
                IsDeleted = false
            };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(5))
                .ReturnsAsync(existingModule);

            _mockCommandRepo
                .Setup(r => r.DeleteModuleAsync(5))
                .Returns(Task.CompletedTask);

            _mockCommandRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(r => r.DeleteModuleAsync(5), Times.Once);
        }
    }
}
