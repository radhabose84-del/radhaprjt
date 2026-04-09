using AutoMapper;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Queries
{
    public sealed class ApprovedIndentDetailsForPOQueryHandlerTests
    {
        private readonly Mock<IPurchaseIndentQuery> _mockIndentQuery = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IInventoryCategoryLookup> _mockCategoryLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseOrderQueryRepository> _mockPoQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDutyMasterQueryRepository> _mockDutyRepo = new(MockBehavior.Loose);

        private ApprovedIndentDetailsForPOQueryHandler CreateSut() =>
            new(_mockIndentQuery.Object, _mockMediator.Object, _mockMapper.Object,
                _mockItemLookup.Object, _mockUomLookup.Object, _mockCategoryLookup.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object, _mockPoQueryRepo.Object,
                _mockDutyRepo.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockIndentQuery
                .Setup(r => r.GetApprovedIndentDetailsForPO(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IndentForPODto>());

            var result = await CreateSut().Handle(
                new ApprovedIndentDetailsForPOQuery { VendorId = 1, DepartmentId = 1 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithData_ReturnsEnrichedList()
        {
            var indents = new List<IndentForPODto>
            {
                new IndentForPODto
                {
                    IndentDetails = new List<IndentDetailsForPODto>(),
                    IndentDutyDetails = new List<IndentDutyForPODto>()
                }
            };
            _mockIndentQuery
                .Setup(r => r.GetApprovedIndentDetailsForPO(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(indents);

            var result = await CreateSut().Handle(
                new ApprovedIndentDetailsForPOQuery { VendorId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
