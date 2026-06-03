using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocation;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.BarcodeAllocation.Queries
{
    public sealed class GetBarcodeAllocationQueryHandlerTests
    {
        private readonly Mock<IBarcodeAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeAllocationQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var data = new List<BarcodeAllocationDto> { BarcodeAllocationBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

            var result = await CreateSut().Handle(new GetBarcodeAllocationQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var data = new List<BarcodeAllocationDto> { BarcodeAllocationBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "BBA")).ReturnsAsync((data, 11));

            var result = await CreateSut().Handle(new GetBarcodeAllocationQuery { PageNumber = 2, PageSize = 5, SearchTerm = "BBA" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<BarcodeAllocationDto>(), 0));

            var result = await CreateSut().Handle(new GetBarcodeAllocationQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().BeEmpty();
        }
    }
}
