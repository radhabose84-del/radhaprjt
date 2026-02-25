#nullable disable
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;
using SalesManagement.Application.SalesOffice.Queries.GetAllSalesOffice;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOffice.Queries
{
    public class GetAllSalesOfficeQueryHandlerTests
    {
        private readonly Mock<ISalesOfficeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllSalesOfficeQueryHandler CreateSut() =>
            new GetAllSalesOfficeQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var query = new GetAllSalesOfficeQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var dtoList = new List<SalesOfficeDto>
            {
                SalesOfficeBuilders.ValidDto(id: 1, name: "Office One"),
                SalesOfficeBuilders.ValidDto(id: 2, name: "Office Two")
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 2));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var query = new GetAllSalesOfficeQuery { PageNumber = 2, PageSize = 5, SearchTerm = "Office" };
            var dtoList = new List<SalesOfficeDto> { SalesOfficeBuilders.ValidDto(id: 6, name: "Office Six") };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "Office")).ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(11);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var query = new GetAllSalesOfficeQuery { PageNumber = 1, PageSize = 10, SearchTerm = "NOTFOUND" };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, "NOTFOUND")).ReturnsAsync((new List<SalesOfficeDto>(), 0));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            var query = new GetAllSalesOfficeQuery { PageNumber = 3, PageSize = 20, SearchTerm = "test" };
            _mockQueryRepo.Setup(r => r.GetAllAsync(3, 20, "test")).ReturnsAsync((new List<SalesOfficeDto>(), 0));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(3, 20, "test"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessMessage()
        {
            var query = new GetAllSalesOfficeQuery { PageNumber = 1, PageSize = 10 };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<SalesOfficeDto>(), 0));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
        }
    }
}
