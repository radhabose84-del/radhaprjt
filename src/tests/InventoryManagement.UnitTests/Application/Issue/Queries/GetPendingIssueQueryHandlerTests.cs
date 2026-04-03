using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IIssue;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Issue.Queries.GetPendingIssue;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.Issue.Queries
{
    public sealed class GetPendingIssueQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWHLookup = new(MockBehavior.Loose);
        private readonly Mock<IRackLookup> _mockRackLookup = new(MockBehavior.Loose);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);

        private GetPendingIssueQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object, _mockWHLookup.Object,
                _mockRackLookup.Object, _mockBinLookup.Object, _mockMiscRepo.Object);

        private void SetupEmptyResult()
        {
            _mockQueryRepo.Setup(r => r.GetPendingIssuesAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<GetPendingIssueDto>());
            _mockMapper.Setup(m => m.Map<List<GetPendingIssueDto>>(It.IsAny<object>()))
                .Returns(new List<GetPendingIssueDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupEmptyResult();

            var result = await CreateSut().Handle(new GetPendingIssueQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsGetPendingIssuesOnce()
        {
            SetupEmptyResult();

            await CreateSut().Handle(new GetPendingIssueQuery { MrsNo = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetPendingIssuesAsync(It.IsAny<int>()), Times.Once);
        }
    }
}
