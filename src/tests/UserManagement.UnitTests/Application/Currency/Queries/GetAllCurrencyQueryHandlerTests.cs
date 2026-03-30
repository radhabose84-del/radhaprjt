using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Currency.Queries
{
    public sealed class GetAllCurrencyQueryHandlerTests
    {
        private readonly Mock<ICurrencyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetCurrencyQueryHandler>> _mockLogger = new();

        private GetCurrencyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Currency> { CurrencyBuilders.ValidEntity() };
            var dtos = new List<CurrencyDto> { CurrencyBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllCurrencyAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<CurrencyDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetCurrencyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Currency> { CurrencyBuilders.ValidEntity() };
            var dtos = new List<CurrencyDto> { CurrencyBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllCurrencyAsync(2, 5, "USD"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<CurrencyDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetCurrencyQuery { PageNumber = 2, PageSize = 5, SearchTerm = "USD" },
                CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Currency>();
            var dtos = new List<CurrencyDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllCurrencyAsync(1, 10, null))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<CurrencyDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetCurrencyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Currency> { CurrencyBuilders.ValidEntity() };
            var dtos = new List<CurrencyDto> { CurrencyBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllCurrencyAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<CurrencyDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetCurrencyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetCurrencyQuery" &&
                        e.Module == "Currency"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
