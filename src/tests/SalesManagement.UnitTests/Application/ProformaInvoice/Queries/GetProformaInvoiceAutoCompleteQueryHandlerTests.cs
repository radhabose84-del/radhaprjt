using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceAutoComplete;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Queries
{
    public sealed class GetProformaInvoiceAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProformaInvoiceAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("term", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProformaInvoiceLookupDto>)new List<ProformaInvoiceLookupDto>
                {
                    new() { Id = 1 }
                });

            var result = await CreateSut().Handle(new GetProformaInvoiceAutoCompleteQuery("term"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepo()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProformaInvoiceLookupDto>)new List<ProformaInvoiceLookupDto>());

            await CreateSut().Handle(new GetProformaInvoiceAutoCompleteQuery(null!), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("x", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProformaInvoiceLookupDto>)new List<ProformaInvoiceLookupDto>());

            await CreateSut().Handle(new GetProformaInvoiceAutoCompleteQuery("x"), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
