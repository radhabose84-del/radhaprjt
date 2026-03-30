using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ITimeZones;
using UserManagement.Application.TimeZones.Queries.GetTimeZones;
using UserManagement.Application.TimeZones.Queries.GetTimeZonesAutoComplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.TimeZones.Queries
{
    public sealed class GetTimeZonesAutocompleteQueryHandlerTests
    {
        private readonly Mock<ITimeZonesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetTimeZonesAutocompleteQueryHandler>> _mockLogger = new();

        private GetTimeZonesAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_MatchingResults_ReturnsDtoList()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.TimeZones>
            {
                new() { Id = 1, Name = "India Standard Time" }
            };
            var dtos = new List<TimeZonesAutoCompleteDto>
            {
                new() { Id = 1, Name = "India Standard Time" }
            };

            _mockQueryRepo
                .Setup(r => r.GetByTimeZonesNameAsync("India"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<TimeZonesAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetTimeZonesAutocompleteQuery { SearchPattern = "India" },
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("India Standard Time");
        }

        [Fact]
        public async Task Handle_NoMatchingResults_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetByTimeZonesNameAsync("xyz"))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.TimeZones>());

            var sut = CreateSut();

            // Act
            Func<Task> act = async () =>
                await sut.Handle(
                    new GetTimeZonesAutocompleteQuery { SearchPattern = "xyz" },
                    CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No TimeZones found*");
        }

        [Fact]
        public async Task Handle_MatchingResults_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.TimeZones>
            {
                new() { Id = 1, Name = "India Standard Time" }
            };
            var dtos = new List<TimeZonesAutoCompleteDto> { new() { Id = 1, Name = "India Standard Time" } };

            _mockQueryRepo
                .Setup(r => r.GetByTimeZonesNameAsync("India"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<TimeZonesAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetTimeZonesAutocompleteQuery { SearchPattern = "India" },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetTimeZonesAutocompleteQuery" &&
                        e.Module == "TimeZones"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MatchingResults_CallsRepositoryOnce()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.TimeZones>
            {
                new() { Id = 1, Name = "India Standard Time" }
            };
            var dtos = new List<TimeZonesAutoCompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetByTimeZonesNameAsync("India"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<TimeZonesAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetTimeZonesAutocompleteQuery { SearchPattern = "India" },
                CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetByTimeZonesNameAsync("India"), Times.Once);
        }
    }
}
