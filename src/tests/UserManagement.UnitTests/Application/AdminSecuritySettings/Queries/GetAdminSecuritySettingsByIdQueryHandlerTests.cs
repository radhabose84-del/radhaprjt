using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettingsById;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.AdminSecuritySettings.Queries
{
    public sealed class GetAdminSecuritySettingsByIdQueryHandlerTests
    {
        private readonly Mock<IAdminSecuritySettingsQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetAdminSecuritySettingsByIdQueryHandler>> _mockLogger = new();

        private GetAdminSecuritySettingsByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.AdminSecuritySettings
            {
                Id = 1,
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3
            };
            var dto = new GetAdminSecuritySettingsDto { Id = 1, SessionTimeoutMinutes = 30 };

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetAdminSecuritySettingsDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetAdminSecuritySettingsByIdQuery { Id = 1 },
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.SessionTimeoutMinutes.Should().Be(30);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.AdminSecuritySettings?)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(
                new GetAdminSecuritySettingsByIdQuery { Id = 999 },
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 5 };
            var dto = new GetAdminSecuritySettingsDto { Id = 5 };

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(5))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetAdminSecuritySettingsDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetAdminSecuritySettingsByIdQuery { Id = 5 },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "Admin Security Settings"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_CallsRepositoryOnce()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.AdminSecuritySettings { Id = 7 };
            var dto = new GetAdminSecuritySettingsDto { Id = 7 };

            _mockQueryRepo
                .Setup(r => r.GetAdminSecuritySettingsByIdAsync(7))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetAdminSecuritySettingsDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetAdminSecuritySettingsByIdQuery { Id = 7 },
                CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(
                r => r.GetAdminSecuritySettingsByIdAsync(7),
                Times.Once);
        }
    }
}
