using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.CostCenter.Queries
{
    public sealed class GetCostCenterQueryHandlerTests
    {
        private readonly Mock<ICostCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetCostCenterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<CostCenterDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllCostCenterListGroupAsync(1, 10, null)).ReturnsAsync((dtos, 1));

            var result = await CreateSut().Handle(new GetCostCenterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsPaginationMetadata()
        {
            var dtos = new List<CostCenterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllCostCenterListGroupAsync(2, 5, "test")).ReturnsAsync((dtos, 11));

            var result = await CreateSut().Handle(new GetCostCenterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllCostCenterListGroupAsync(1, 10, null)).ReturnsAsync((new List<CostCenterDto>(), 0));

            var result = await CreateSut().Handle(new GetCostCenterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
