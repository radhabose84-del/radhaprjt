using AutoMapper;
using MediatR;
using SalesManagement.Domain.Events;
using Contracts.Dtos.Lookups.Sales;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitAutoComplete;

namespace SalesManagement.UnitTests.Application.CommissionSplit.Queries
{
    public sealed class GetCommissionSplitAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCommissionSplitAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("term", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CommissionSplitLookupDto>)new List<CommissionSplitLookupDto>
                {
                    new() { Id = 1 }
                });

            var result = await CreateSut().Handle(new GetCommissionSplitAutoCompleteQuery("term"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepo()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CommissionSplitLookupDto>)new List<CommissionSplitLookupDto>());

            await CreateSut().Handle(new GetCommissionSplitAutoCompleteQuery(null!), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("x", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CommissionSplitLookupDto>)new List<CommissionSplitLookupDto>());

            await CreateSut().Handle(new GetCommissionSplitAutoCompleteQuery("x"), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
