using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.AdminSecuritySettings.Commands.CreateAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.AdminSecuritySettings.Commands
{
    public sealed class CreateAdminSecuritySettingsCommandHandlerTests
    {
        private readonly Mock<IAdminSecuritySettingsCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateAdminSecuritySettingsCommandHandler>> _mockLogger = new();

        private CreateAdminSecuritySettingsCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private static CreateAdminSecuritySettingsCommand ValidCommand() => new CreateAdminSecuritySettingsCommand
        {
            PasswordHistoryCount = 5,
            SessionTimeoutMinutes = 30,
            MaxFailedLoginAttempts = 3,
            AccountAutoUnlockMinutes = 15,
            PasswordExpiryDays = 90,
            PasswordExpiryAlertDays = 7,
            IsTwoFactorAuthenticationEnabled = 1,
            MaxConcurrentLogins = 1,
            IsForcePasswordChangeOnFirstLogin = 1,
            PasswordResetCodeExpiryMinutes = 10,
            IsCaptchaEnabledOnLogin = 0
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };
            var dto = new AdminSecuritySettingsDto { Id = 1 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AdminSecuritySettingsDto>(entity))
                .Returns(dto);

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
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 2 };
            var dto = new AdminSecuritySettingsDto { Id = 2 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AdminSecuritySettingsDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.AdminSecuritySettings>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 3 };
            var dto = new AdminSecuritySettingsDto { Id = 3 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AdminSecuritySettingsDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Admin Security Settings"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
