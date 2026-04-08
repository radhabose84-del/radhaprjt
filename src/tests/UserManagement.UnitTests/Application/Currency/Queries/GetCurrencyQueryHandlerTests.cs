using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Currency.Queries
{
    public sealed class GetCurrencyQueryHandlerTests
    {
        private readonly Mock<ICurrencyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetCurrencyQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetCurrencyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Currency> { new() { Id = 1 } };
            var dtoList = new List<CurrencyDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllCurrencyAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<CurrencyDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCurrencyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.Currency>();

            _mockQueryRepo
                .Setup(r => r.GetAllCurrencyAsync(2, 5, "USD"))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<CurrencyDto>>(entities))
                .Returns(new List<CurrencyDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCurrencyQuery { PageNumber = 2, PageSize = 5, SearchTerm = "USD" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}
