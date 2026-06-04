using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Dto;
using FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterAutoComplete;

namespace FinanceManagement.UnitTests.Application.TransactionTypeMaster.Queries
{
    public sealed class GetTransactionTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ITransactionTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetTransactionTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingResults()
        {
            var lookupList = new List<TransactionTypeMasterLookupDto>
            {
                new() { Id = 1, TypeName = "Invoice", ShortName = "INV" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Inv", It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<TransactionTypeMasterLookupDto>>(lookupList))
                .Returns(lookupList);

            var result = await CreateSut().Handle(
                new GetTransactionTypeMasterAutoCompleteQuery("Inv"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].TypeName.Should().Be("Invoice");
        }

        [Fact]
        public async Task Handle_EmptyTerm_PassesEmptyString()
        {
            var emptyList = new List<TransactionTypeMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);
            _mockMapper
                .Setup(m => m.Map<List<TransactionTypeMasterLookupDto>>(emptyList))
                .Returns(emptyList);

            // Term is non-nullable in this record, but handler uses ?? string.Empty
            var result = await CreateSut().Handle(
                new GetTransactionTypeMasterAutoCompleteQuery(""), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookupList = new List<TransactionTypeMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<TransactionTypeMasterLookupDto>>(lookupList))
                .Returns(lookupList);

            await CreateSut().Handle(
                new GetTransactionTypeMasterAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
