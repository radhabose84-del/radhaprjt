using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Arrival.Queries.GetAllArrival;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.Arrival.Queries
{
    public sealed class GetAllArrivalQueryHandlerTests
    {
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllArrivalQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var list = new List<ArrivalDto> { ArrivalBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null)).ReturnsAsync((list, 1));

            var result = await CreateSut().Handle(
                new GetAllArrivalQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var list = new List<ArrivalDto> { ArrivalBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "ARV", null)).ReturnsAsync((list, 11));

            var result = await CreateSut().Handle(
                new GetAllArrivalQuery { PageNumber = 2, PageSize = 5, SearchTerm = "ARV" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null)).ReturnsAsync((new List<ArrivalDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllArrivalQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ForwardsPendingStatusFilter()
        {
            var list = new List<ArrivalDto> { ArrivalBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, true)).ReturnsAsync((list, 1));

            var result = await CreateSut().Handle(
                new GetAllArrivalQuery { PageNumber = 1, PageSize = 10, PendingStatus = true }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, null, true), Times.Once);
        }
    }
}
