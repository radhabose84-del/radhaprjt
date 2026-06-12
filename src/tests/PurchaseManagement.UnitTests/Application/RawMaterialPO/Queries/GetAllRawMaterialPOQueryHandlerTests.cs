using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetAllRawMaterialPO;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Queries
{
    public sealed class GetAllRawMaterialPOQueryHandlerTests
    {
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllRawMaterialPOQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var list = new List<RawMaterialPODto> { RawMaterialPOBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null, null)).ReturnsAsync((list, 1));

            var result = await CreateSut().Handle(
                new GetAllRawMaterialPOQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var list = new List<RawMaterialPODto> { RawMaterialPOBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "RMPO", null, null)).ReturnsAsync((list, 11));

            var result = await CreateSut().Handle(
                new GetAllRawMaterialPOQuery { PageNumber = 2, PageSize = 5, SearchTerm = "RMPO" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null, null)).ReturnsAsync((new List<RawMaterialPODto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllRawMaterialPOQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
