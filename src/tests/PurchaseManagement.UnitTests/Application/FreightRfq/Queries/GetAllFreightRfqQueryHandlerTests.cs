using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Application.FreightRfq.Queries.GetAllFreightRfq;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.FreightRfq.Queries
{
    public sealed class GetAllFreightRfqQueryHandlerTests
    {
        private readonly Mock<IFreightRfqQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllFreightRfqQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var list = new List<FreightRfqListDto> { FreightRfqBuilders.ValidListDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((list, 1));

            var result = await CreateSut().Handle(
                new GetAllFreightRfqQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_PassesPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "x", 3))
                .ReturnsAsync((new List<FreightRfqListDto>(), 11));

            var result = await CreateSut().Handle(
                new GetAllFreightRfqQuery { PageNumber = 2, PageSize = 5, SearchTerm = "x", StatusId = 3 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
