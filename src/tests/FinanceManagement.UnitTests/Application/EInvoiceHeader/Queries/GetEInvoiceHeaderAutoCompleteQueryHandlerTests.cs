using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderAutoComplete;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Queries
{
    public sealed class GetEInvoiceHeaderAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEInvoiceHeaderAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingResults()
        {
            var lookupList = new List<EInvoiceHeaderLookupDto>
            {
                new() { Id = 1, InvoiceNo = "INV001", IrnNumber = "IRN123" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("INV", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<EInvoiceHeaderLookupDto>>(lookupList))
                .Returns(lookupList);

            var result = await CreateSut().Handle(
                new GetEInvoiceHeaderAutoCompleteQuery("INV"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].InvoiceNo.Should().Be("INV001");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var emptyList = new List<EInvoiceHeaderLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("xyz", It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);
            _mockMapper
                .Setup(m => m.Map<List<EInvoiceHeaderLookupDto>>(emptyList))
                .Returns(emptyList);

            var result = await CreateSut().Handle(
                new GetEInvoiceHeaderAutoCompleteQuery("xyz"), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookupList = new List<EInvoiceHeaderLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<EInvoiceHeaderLookupDto>>(lookupList))
                .Returns(lookupList);

            await CreateSut().Handle(
                new GetEInvoiceHeaderAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
