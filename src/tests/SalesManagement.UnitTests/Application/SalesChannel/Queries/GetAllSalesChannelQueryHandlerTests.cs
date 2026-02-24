#nullable disable
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;
using SalesManagement.Application.SalesChannel.Queries.GetAllSalesChannel;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesChannel.Queries
{
    public class GetAllSalesChannelQueryHandlerTests
    {
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllSalesChannelQueryHandler CreateSut() =>
            new GetAllSalesChannelQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var query = new GetAllSalesChannelQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var dtoList = new List<SalesChannelDto>
            {
                SalesChannelBuilders.ValidDto(id: 1, code: "CH001"),
                SalesChannelBuilders.ValidDto(id: 2, code: "CH002")
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 2));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            var query = new GetAllSalesChannelQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var dtoList = new List<SalesChannelDto>
            {
                SalesChannelBuilders.ValidDto(id: 1, code: "CH001"),
                SalesChannelBuilders.ValidDto(id: 2, code: "CH002")
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 2));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().HaveCount(2);
            result.Data[0].SalesChannelCode.Should().Be("CH001");
            result.Data[1].SalesChannelCode.Should().Be("CH002");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var query = new GetAllSalesChannelQuery { PageNumber = 2, PageSize = 5, SearchTerm = "CH" };
            var dtoList = new List<SalesChannelDto> { SalesChannelBuilders.ValidDto(id: 6, code: "CH006") };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "CH")).ReturnsAsync((dtoList, 6));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(6);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            var query = new GetAllSalesChannelQuery { PageNumber = 1, PageSize = 10, SearchTerm = "NOTFOUND" };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, "NOTFOUND")).ReturnsAsync((new List<SalesChannelDto>(), 0));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            var query = new GetAllSalesChannelQuery { PageNumber = 3, PageSize = 20, SearchTerm = "test" };
            _mockQueryRepo.Setup(r => r.GetAllAsync(3, 20, "test")).ReturnsAsync((new List<SalesChannelDto>(), 0));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(3, 20, "test"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessMessage()
        {
            var query = new GetAllSalesChannelQuery { PageNumber = 1, PageSize = 10 };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<SalesChannelDto>(), 0));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
        }
    }
}
