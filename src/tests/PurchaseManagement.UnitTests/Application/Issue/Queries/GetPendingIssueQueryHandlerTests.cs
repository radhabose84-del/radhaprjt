using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssue;

namespace PurchaseManagement.UnitTests.Application.Issue.Queries
{
    public sealed class GetPendingIssueQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);
        private readonly Mock<IRackLookup> _mockRackLookup = new(MockBehavior.Loose);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterLookup> _mockMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseToleranceLookup> _mockToleranceLookup = new(MockBehavior.Loose);

        private GetPendingIssueQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object,
                _mockUomLookup.Object, _mockWarehouseLookup.Object, _mockRackLookup.Object,
                _mockBinLookup.Object, _mockMiscLookup.Object, _mockToleranceLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPendingIssueQuery { MrsNo = 42 };
            query.MrsNo.Should().Be(42);
        }
    }
}
