#nullable disable
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Queries.GetAllSalesItemPriceMaster;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesItemPriceMaster.Queries
{
    public class GetAllSalesItemPriceMasterQueryHandlerTests
    {
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllSalesItemPriceMasterQueryHandler CreateSut() =>
            new GetAllSalesItemPriceMasterQueryHandler(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsPagedResult_WithCorrectData()
        {
            var list = new List<SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterDto>
            {
                SalesItemPriceMasterBuilders.ValidDto(id: 1),
                SalesItemPriceMasterBuilders.ValidDto(id: 2, priceCode: "PC002")
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((list, 2));

            var query = new GetAllSalesItemPriceMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterDto>(), 0));

            var query = new GetAllSalesItemPriceMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PassesSearchTermToRepository()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, "PC"))
                .ReturnsAsync((new List<SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterDto>(), 0));

            var query = new GetAllSalesItemPriceMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = "PC" };

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, "PC"), Times.Once);
        }

        [Fact]
        public async Task Handle_PageNumberAndSizeReflectedInResult()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(3, 5, null))
                .ReturnsAsync((new List<SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterDto>(), 0));

            var query = new GetAllSalesItemPriceMasterQuery { PageNumber = 3, PageSize = 5, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(5);
        }
    }
}
