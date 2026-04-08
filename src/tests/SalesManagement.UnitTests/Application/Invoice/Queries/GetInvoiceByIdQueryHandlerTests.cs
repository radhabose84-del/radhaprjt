using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetInvoiceById;

namespace SalesManagement.UnitTests.Application.Invoice.Queries
{
    public sealed class GetInvoiceByIdQueryHandlerTests
    {
        private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetInvoiceByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new InvoiceHeaderDto { Id = 1 });

            var result = await CreateSut().Handle(new GetInvoiceByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((InvoiceHeaderDto?)null);

            var result = await CreateSut().Handle(new GetInvoiceByIdQuery { Id = 99 }, CancellationToken.None);

            // Handler returns null! for null case
            result.Should().BeNull();
        }
    }
}
