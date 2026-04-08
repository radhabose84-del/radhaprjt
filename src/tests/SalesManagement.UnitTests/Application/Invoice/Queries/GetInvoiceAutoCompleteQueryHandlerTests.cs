using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetInvoiceAutoComplete;

namespace SalesManagement.UnitTests.Application.Invoice.Queries
{
    public sealed class GetInvoiceAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetInvoiceAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<InvoiceLookupDto> { new() } as IReadOnlyList<InvoiceLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("INV", It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetInvoiceAutoCompleteQuery("INV"), CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
