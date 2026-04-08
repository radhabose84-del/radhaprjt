using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetInvoicePrintDetails;

namespace SalesManagement.UnitTests.Application.Invoice.Queries
{
    public sealed class GetInvoicePrintDetailsQueryHandlerTests
    {
        private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetInvoicePrintDetailsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetPrintDetailsAsync(1)).ReturnsAsync(new InvoicePrintDto());

            var result = await CreateSut().Handle(new GetInvoicePrintDetailsQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetPrintDetailsAsync(99)).ReturnsAsync((InvoicePrintDto?)null);

            var result = await CreateSut().Handle(new GetInvoicePrintDetailsQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
