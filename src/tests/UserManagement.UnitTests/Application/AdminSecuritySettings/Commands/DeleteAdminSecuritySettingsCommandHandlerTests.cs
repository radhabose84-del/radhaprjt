using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.AdminSecuritySettings.Commands
{
    public sealed class DeleteAdminSecuritySettingsCommandHandlerTests
    {
        private readonly Mock<IAdminSecuritySettingsCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAdminSecuritySettingsQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<DeleteAdminSecuritySettingsCommandHandler>> _mockLogger = new();

        private DeleteAdminSecuritySettingsCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsPositiveResult()
        {
            // Arrange
            var command = new DeleteAdminSecuritySettingsCommand { Id = 1 };
            var existing = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };
            var mappedEntity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(1))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, mappedEntity))
                .ReturnsAsync(1);

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
            var command = new DeleteAdminSecuritySettingsCommand { Id = 999 };

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
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            // Arrange
            var command = new DeleteAdminSecuritySettingsCommand { Id = 5 };
            var existing = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 5 };
            var mappedEntity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 5 };

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(5))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(5, mappedEntity))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsZero_ReturnsZero()
        {
            // Arrange
            var command = new DeleteAdminSecuritySettingsCommand { Id = 7 };
            var existing = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 7 };
            var mappedEntity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 7 };

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(7))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(7, mappedEntity))
                .ReturnsAsync(0);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(0);
        }
    }
}
