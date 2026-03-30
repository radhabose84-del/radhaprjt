using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.AdminSecuritySettings.Commands
{
    public sealed class UpdateAdminSecuritySettingsCommandHandlerTests
    {
        private readonly Mock<IAdminSecuritySettingsCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAdminSecuritySettingsQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateAdminSecuritySettingsCommandHandler>> _mockLogger = new();

        private UpdateAdminSecuritySettingsCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private static UpdateAdminSecuritySettingsCommand ValidCommand(int id = 1) =>
            new UpdateAdminSecuritySettingsCommand
            {
                Id = id,
                PasswordHistoryCount = 5,
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3,
                AccountAutoUnlockMinutes = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlertDays = 7,
                MaxConcurrentLogins = 1,
                IsActive = 1
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            // Arrange
            var command = ValidCommand();
            var existing = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };
            var entityToUpdate = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };
            var updatedEntity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };

            _mockQueryRepo
                .SetupSequence(r => r.GetAdminSecuritySettingsByIdAsync(1))
                .ReturnsAsync(existing)
                .ReturnsAsync(updatedEntity);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(entityToUpdate);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, entityToUpdate))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<AdminSecuritySettingsDto>(It.IsAny<UserManagement.Domain.Entities.AdminSecuritySettings>()))
                .Returns(new AdminSecuritySettingsDto { Id = 1 });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            // Arrange
            var command = ValidCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.AdminSecuritySettings?)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_UpdateReturnsMinusOne_ThrowsException()
        {
            // Arrange
            var command = ValidCommand();
            var existing = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };
            var entityToUpdate = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(1))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(entityToUpdate);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, entityToUpdate))
                .ReturnsAsync(-1);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Failed*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = ValidCommand();
            var existing = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };
            var entityToUpdate = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };
            var updatedEntity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };

            _mockQueryRepo
                .SetupSequence(r => r.GetAdminSecuritySettingsByIdAsync(1))
                .ReturnsAsync(existing)
                .ReturnsAsync(updatedEntity);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(entityToUpdate);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, entityToUpdate))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<AdminSecuritySettingsDto>(It.IsAny<UserManagement.Domain.Entities.AdminSecuritySettings>()))
                .Returns(new AdminSecuritySettingsDto { Id = 1 });

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
