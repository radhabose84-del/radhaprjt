#nullable disable
using SalesManagement.Application.BusinessUnit.Queries.GetAllBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.BusinessUnit.Queries
{
    public class GetAllBusinessUnitQueryHandlerTests
    {
        private readonly Mock<IBusinessUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllBusinessUnitQueryHandler CreateSut() =>
            new GetAllBusinessUnitQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var data = new List<SalesManagement.Application.BusinessUnit.Dto.BusinessUnitDto>
            {
                BusinessUnitBuilders.ValidDto(id: 1, code: "BU001"),
                BusinessUnitBuilders.ValidDto(id: 2, code: "BU002")
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var query = new GetAllBusinessUnitQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            var data = new List<SalesManagement.Application.BusinessUnit.Dto.BusinessUnitDto>
            {
                BusinessUnitBuilders.ValidDto(id: 1, code: "BU001"),
                BusinessUnitBuilders.ValidDto(id: 2, code: "BU002")
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var query = new GetAllBusinessUnitQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().HaveCount(2);
            result.Data[0].BusinessUnitCode.Should().Be("BU001");
            result.Data[1].BusinessUnitCode.Should().Be("BU002");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<SalesManagement.Application.BusinessUnit.Dto.BusinessUnitDto>
            {
                BusinessUnitBuilders.ValidDto(id: 1)
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "BU"))
                .ReturnsAsync((data, 15));

            var query = new GetAllBusinessUnitQuery { PageNumber = 2, PageSize = 5, SearchTerm = "BU" };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(15);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<SalesManagement.Application.BusinessUnit.Dto.BusinessUnitDto>(), 0));

            var query = new GetAllBusinessUnitQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, "test"))
                .ReturnsAsync((new List<SalesManagement.Application.BusinessUnit.Dto.BusinessUnitDto>(), 0));

            var query = new GetAllBusinessUnitQuery { PageNumber = 1, PageSize = 20, SearchTerm = "test" };
            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, "test"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessMessage()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<SalesManagement.Application.BusinessUnit.Dto.BusinessUnitDto>(), 0));

            var query = new GetAllBusinessUnitQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
        }
    }
}
