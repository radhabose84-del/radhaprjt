using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationById;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.BarcodeAllocation.Queries
{
    public sealed class GetBarcodeAllocationByIdQueryHandlerTests
    {
        private readonly Mock<IBarcodeAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeAllocationByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BarcodeAllocationBuilders.ValidDto(1));

            var result = await CreateSut().Handle(new GetBarcodeAllocationByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BarcodeAllocationDto?)null);

            var result = await CreateSut().Handle(new GetBarcodeAllocationByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
