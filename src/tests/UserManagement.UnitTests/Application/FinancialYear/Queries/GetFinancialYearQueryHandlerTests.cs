using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Application.GetFinancialYearYear.Queries.GetFinancialYear;
using UserManagement.Domain.Events;
using GetFinancialYearQueryHandler = UserManagement.Application.GetFinancialYear.Queries.GetFinancialYear.GetFinancialYearQueryHandler;

namespace UserManagement.UnitTests.Application.FinancialYear.Queries
{
    public sealed class GetFinancialYearQueryHandlerTests
    {
        private readonly Mock<IFinancialYearQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetFinancialYearQueryHandler>> _mockLogger = new();

        private GetFinancialYearQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithRecords_ReturnsSuccess()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.FinancialYear>
            {
                new() { Id = 1, StartYear = "2024-25" }
            };
            var dtos = new List<GetFinancialYearDto> { new GetFinancialYearDto { Id = 1, StartYear = "2024-25" } };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialYearAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetFinancialYearDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetFinancialYearQuery { PageNumber = 1, PageSize = 10 },
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
                .Setup(r => r.GetAllFinancialYearAsync(1, 10, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.FinancialYear>(), 0));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetFinancialYearQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("No Record Found");
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.FinancialYear>
            {
                new() { Id = 2, StartYear = "2025-26" }
            };
            var dtos = new List<GetFinancialYearDto> { new GetFinancialYearDto { Id = 2, StartYear = "2025-26" } };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialYearAsync(2, 5, "2025"))
                .ReturnsAsync((entities, 20));

            _mockMapper
                .Setup(m => m.Map<List<GetFinancialYearDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetFinancialYearQuery { PageNumber = 2, PageSize = 5, SearchTerm = "2025" },
                CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }
    }
}
