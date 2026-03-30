using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ITimeZones;
using UserManagement.Application.TimeZones.Queries.GetTimeZones;
using UserManagement.Application.TimeZones.Queries.GetTimeZonesById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.TimeZones.Queries
{
    public sealed class GetTimeZoneByIdQueryHandlerTests
    {
        private readonly Mock<ITimeZonesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetTimeZoneByIdQueryHandler>> _mockLogger = new();

        private GetTimeZoneByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.TimeZones { Id = 1, Code = "IST", Name = "India Standard Time" };
            var dto = new TimeZonesDto { Id = 1, Code = "IST", Name = "India Standard Time" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<TimeZonesDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(new GetTimeZoneByIdQuery { TimeZoneId = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Code.Should().Be("IST");
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.TimeZones)null!);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () =>
                await sut.Handle(new GetTimeZoneByIdQuery { TimeZoneId = 999 }, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No TimeZones found*");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.TimeZones { Id = 5, Code = "UTC", Name = "Coordinated Universal Time" };
            var dto = new TimeZonesDto { Id = 5, Code = "UTC", Name = "Coordinated Universal Time" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<TimeZonesDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(new GetTimeZoneByIdQuery { TimeZoneId = 5 }, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetTimeZoneByIdQuery" &&
                        e.Module == "TimeZones"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsRepositoryOnce()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.TimeZones { Id = 1, Code = "IST", Name = "India Standard Time" };
            var dto = new TimeZonesDto { Id = 1, Code = "IST" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<TimeZonesDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(new GetTimeZoneByIdQuery { TimeZoneId = 1 }, CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
