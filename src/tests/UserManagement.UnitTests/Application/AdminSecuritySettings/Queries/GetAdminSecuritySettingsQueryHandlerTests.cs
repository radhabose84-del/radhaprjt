using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.AdminSecuritySettings.Queries
{
    public sealed class GetAdminSecuritySettingsQueryHandlerTests
    {
        private readonly Mock<IAdminSecuritySettingsQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetAdminSecuritySettingsQueryHandler>> _mockLogger = new();

        private GetAdminSecuritySettingsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithRecords_ReturnsSuccess()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.AdminSecuritySettings>
            {
                new() { Id = 1, SessionTimeoutMinutes = 30 }
            };
            var dtos = new List<GetAdminSecuritySettingsDto>
            {
                new() { Id = 1, SessionTimeoutMinutes = 30 }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAdminSecuritySettingsAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetAdminSecuritySettingsDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetAdminSecuritySettingsQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetAllAdminSecuritySettingsAsync(1, 10, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.AdminSecuritySettings>(), 0));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetAdminSecuritySettingsQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("No Record Found");
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.AdminSecuritySettings>
            {
                new() { Id = 2 }
            };
            var dtos = new List<GetAdminSecuritySettingsDto> { new() { Id = 2 } };

            _mockQueryRepo
                .Setup(r => r.GetAllAdminSecuritySettingsAsync(2, 5, "session"))
                .ReturnsAsync((entities, 12));

            _mockMapper
                .Setup(m => m.Map<List<GetAdminSecuritySettingsDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetAdminSecuritySettingsQuery { PageNumber = 2, PageSize = 5, SearchTerm = "session" },
                CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(12);
        }

        [Fact]
        public async Task Handle_WithRecords_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.AdminSecuritySettings>
            {
                new() { Id = 3 }
            };
            var dtos = new List<GetAdminSecuritySettingsDto> { new() { Id = 3 } };

            _mockQueryRepo
                .Setup(r => r.GetAllAdminSecuritySettingsAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetAdminSecuritySettingsDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetAdminSecuritySettingsQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.Module == "AdminSecuritySettings"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
