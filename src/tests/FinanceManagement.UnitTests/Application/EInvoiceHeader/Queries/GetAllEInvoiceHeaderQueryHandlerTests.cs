using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetAllEInvoiceHeader;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Queries
{
    public sealed class GetAllEInvoiceHeaderQueryHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllEInvoiceHeaderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<EInvoiceHeaderDto> { new() { Id = 1, InvoiceNo = "INV001" } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<EInvoiceHeaderDto>>(dtoList)).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllEInvoiceHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<EInvoiceHeaderDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(3, 20, "INV")).ReturnsAsync((dtoList, 50));
            _mockMapper.Setup(m => m.Map<List<EInvoiceHeaderDto>>(dtoList)).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllEInvoiceHeaderQuery { PageNumber = 3, PageSize = 20, SearchTerm = "INV" },
                CancellationToken.None);

            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(20);
            result.TotalCount.Should().Be(50);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var empty = new List<EInvoiceHeaderDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((empty, 0));
            _mockMapper.Setup(m => m.Map<List<EInvoiceHeaderDto>>(empty)).Returns(empty);

            var result = await CreateSut().Handle(
                new GetAllEInvoiceHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<EInvoiceHeaderDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<EInvoiceHeaderDto>>(dtoList)).Returns(dtoList);

            await CreateSut().Handle(
                new GetAllEInvoiceHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
