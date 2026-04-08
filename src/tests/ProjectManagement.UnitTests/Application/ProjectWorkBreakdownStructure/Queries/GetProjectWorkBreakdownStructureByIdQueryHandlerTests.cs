using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetById;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWBSById;
using ProjectManagement.Domain.Events;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Queries
{
    public sealed class GetProjectWorkBreakdownStructureByIdQueryHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCostCenterLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetProjectWorkBreakdownStructureByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockCurrencyLookup.Object, _mockUnitLookup.Object, _mockFyLookup.Object, _mockCostCenterLookup.Object, _mockMediator.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ProjectWorkBreakdownStructureDto?)null);

            var result = await CreateSut().Handle(new GetProjectWorkBreakdownStructureByIdQuery(99), CancellationToken.None);
            result.Should().BeNull();
        }
    }
}
