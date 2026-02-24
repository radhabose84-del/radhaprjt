#nullable disable
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;
using SalesManagement.Application.MiscMaster.Queries.GetAllMiscMaster;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetAllMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllMiscMasterQueryHandler CreateSut() => new(_mockQueryRepo.Object);

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<MiscMasterDto> { MiscMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<MiscMasterDto> { MiscMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search", null))
                .ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((new List<MiscMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_FilterByMiscTypeId_PassesFilterToRepository()
        {
            var dtoList = new List<MiscMasterDto> { MiscMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, 5))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10, MiscTypeId = 5 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithSearchTerm_PassesTermToRepository()
        {
            var dtoList = new List<MiscMasterDto> { MiscMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, "CODE", null))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = "CODE" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }
    }
}
