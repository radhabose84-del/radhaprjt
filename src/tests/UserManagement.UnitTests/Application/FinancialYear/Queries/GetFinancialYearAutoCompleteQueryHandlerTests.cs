using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.FinancialYear.Queries
{
    public sealed class GetFinancialYearAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IFinancialYearCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFinancialYearQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetFinancialYearAutoCompleteQueryHandler>> _mockLogger = new();

        private GetFinancialYearAutoCompleteQueryHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_MatchingTerm_ReturnsDtoList()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.FinancialYear>
            {
                new() { Id = 1, StartYear = "2024-25" }
            };
            var dtos = new List<GetFinancialYearAutoCompleteDto>
            {
                new() { Id = 1, StartYear = "2024-25" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialAutoCompleteSearchAsync("2024"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetFinancialYearAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetFinancialYearAutoCompleteQuery { SearchTerm = "2024" },
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].StartYear.Should().Be("2024-25");
        }

        [Fact]
        public async Task Handle_NoMatch_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetAllFinancialAutoCompleteSearchAsync("NOPE"))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.FinancialYear>());

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(
                new GetFinancialYearAutoCompleteQuery { SearchTerm = "NOPE" },
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No matching*");
        }

        [Fact]
        public async Task Handle_ValidTerm_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.FinancialYear>
            {
                new() { Id = 2, StartYear = "2025-26" }
            };
            var dtos = new List<GetFinancialYearAutoCompleteDto>
            {
                new() { Id = 2, StartYear = "2025-26" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialAutoCompleteSearchAsync("2025"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetFinancialYearAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetFinancialYearAutoCompleteQuery { SearchTerm = "2025" },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetAutoComplete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
