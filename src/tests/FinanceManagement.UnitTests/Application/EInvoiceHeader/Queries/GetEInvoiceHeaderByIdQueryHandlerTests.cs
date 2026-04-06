using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderById;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Queries
{
    public sealed class GetEInvoiceHeaderByIdQueryHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEInvoiceHeaderByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new EInvoiceHeaderDto { Id = 1, InvoiceNo = "INV001" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<EInvoiceHeaderDto>(dto)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetEInvoiceHeaderByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.InvoiceNo.Should().Be("INV001");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((EInvoiceHeaderDto?)null);

            var result = await CreateSut().Handle(
                new GetEInvoiceHeaderByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = new EInvoiceHeaderDto { Id = 1, InvoiceNo = "INV001" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<EInvoiceHeaderDto>(dto)).Returns(dto);

            await CreateSut().Handle(
                new GetEInvoiceHeaderByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((EInvoiceHeaderDto?)null);

            await CreateSut().Handle(
                new GetEInvoiceHeaderByIdQuery { Id = 999 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
