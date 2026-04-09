using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Application.StoReceipt.Queries.GetDcOpenQty;

namespace SalesManagement.UnitTests.Application.StoReceipt.Queries
{
    public sealed class GetDcOpenQtyQueryHandlerTests
    {
        private readonly Mock<IStoReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDcOpenQtyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingDetail_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetDcOpenQtyAsync(1)).ReturnsAsync(new DcOpenQtyDto());

            var result = await CreateSut().Handle(new GetDcOpenQtyQuery { DcDetailId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NonExistentDetail_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetDcOpenQtyAsync(99)).ReturnsAsync((DcOpenQtyDto?)null);

            var result = await CreateSut().Handle(new GetDcOpenQtyQuery { DcDetailId = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
