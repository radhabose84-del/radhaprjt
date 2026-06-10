using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqPending;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.FreightRfq.Queries
{
    public sealed class GetFreightRfqPendingQueryHandlerTests
    {
        private readonly Mock<IFreightRfqQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFreightRfqPendingQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsPendingRows()
        {
            var list = new List<FreightRfqListDto> { FreightRfqBuilders.ValidListDto() };
            _mockQueryRepo.Setup(r => r.GetPendingAsync(1, 10)).ReturnsAsync((list, 1));

            var result = await CreateSut().Handle(
                new GetFreightRfqPendingQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CallsGetPendingOnce()
        {
            _mockQueryRepo.Setup(r => r.GetPendingAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((new List<FreightRfqListDto>(), 0));

            await CreateSut().Handle(new GetFreightRfqPendingQuery { PageNumber = 2, PageSize = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetPendingAsync(2, 5), Times.Once);
        }
    }
}
