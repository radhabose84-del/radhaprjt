#nullable disable
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Dto;
using SalesManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public class GetAllMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllMiscTypeMasterQueryHandler CreateSut() =>
            new GetAllMiscTypeMasterQueryHandler(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var dtoList = new List<MiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            var dtoList = new List<MiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data.Should().HaveCount(1);
            result.Data[0].MiscTypeCode.Should().Be("MISC001");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var dtoList = new List<MiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test")).ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null))
                .ReturnsAsync((new List<MiscTypeMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, "misc"))
                .ReturnsAsync((new List<MiscTypeMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = "misc" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, "misc"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessMessage()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null))
                .ReturnsAsync((new List<MiscTypeMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Message.Should().NotBeNullOrWhiteSpace();
        }
    }
}
