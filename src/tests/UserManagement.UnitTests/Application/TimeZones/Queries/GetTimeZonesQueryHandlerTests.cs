using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ITimeZones;
using UserManagement.Application.TimeZones.Queries.GetTimeZones;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.TimeZones.Queries
{
    public sealed class GetTimeZonesQueryHandlerTests
    {
        private readonly Mock<ITimeZonesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetTimeZonesQueryHandler>> _mockLogger = new();

        private GetTimeZonesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.TimeZones>
            {
                new() { Id = 1, Code = "IST", Name = "India Standard Time" }
            };
            var dtos = new List<TimeZonesDto>
            {
                new() { Id = 1, Code = "IST", Name = "India Standard Time" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllTimeZonesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<TimeZonesDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetTimeZonesQuery { PageNumber = 1, PageSize = 10 },
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
                .Setup(r => r.GetAllTimeZonesAsync(1, 10, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.TimeZones>(), 0));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetTimeZonesQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No TimeZones found");
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsPaginationMetadata()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.TimeZones>
            {
                new() { Id = 1, Code = "IST", Name = "India Standard Time" },
                new() { Id = 2, Code = "UTC", Name = "Coordinated Universal Time" }
            };
            var dtos = new List<TimeZonesDto>
            {
                new() { Id = 1, Code = "IST", Name = "India Standard Time" },
                new() { Id = 2, Code = "UTC", Name = "Coordinated Universal Time" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllTimeZonesAsync(2, 5, "India"))
                .ReturnsAsync((entities, 10));

            _mockMapper
                .Setup(m => m.Map<List<TimeZonesDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetTimeZonesQuery { PageNumber = 2, PageSize = 5, SearchTerm = "India" },
                CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(10);
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.TimeZones>
            {
                new() { Id = 1, Code = "IST", Name = "India Standard Time" }
            };
            var dtos = new List<TimeZonesDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllTimeZonesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<TimeZonesDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetTimeZonesQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetTimeZonesQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
