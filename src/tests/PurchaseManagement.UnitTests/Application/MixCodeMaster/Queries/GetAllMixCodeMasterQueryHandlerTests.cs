using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetAllMixCodeMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MixCodeMaster.Queries
{
    public sealed class GetAllMixCodeMasterQueryHandlerTests
    {
        private readonly Mock<IMixCodeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllMixCodeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var list = new List<MixCodeMasterDto> { MixCodeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((list, 1));

            var result = await CreateSut().Handle(
                new GetAllMixCodeMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var list = new List<MixCodeMasterDto> { MixCodeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "search")).ReturnsAsync((list, 11));

            var result = await CreateSut().Handle(
                new GetAllMixCodeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<MixCodeMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllMixCodeMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
