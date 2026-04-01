using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.MRS.Queries.GetMrsPending;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetMrsPendingQueryHandlerTests
    {
        private readonly Mock<IMrsEntryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private GetMrsPendingQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockUnitLookup.Object, _mockWorkflowLookup.Object, _mockDeptLookup.Object,
                _mockUserLookup.Object, _mockIpService.Object, _mockUomLookup.Object);

        private void SetupDefaults(List<MrsPendingDto> dtos, int totalCount = 0)
        {
            _mockQueryRepo.Setup(r => r.GetPendingMrsDetailsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((dtos, totalCount));
            _mockMapper.Setup(m => m.Map<List<MrsPendingDto>>(It.IsAny<object>())).Returns(dtos);
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUserLookup.Setup(u => u.GetAllUserAsync()).ReturnsAsync(new List<UserLookupDto>());
            _mockUomLookup.Setup(u => u.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UOMLookupDto>());
            _mockWorkflowLookup.Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>());
            _mockIpService.Setup(i => i.GetUserId()).Returns(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsPaginatedResult()
        {
            SetupDefaults(new List<MrsPendingDto>(), 0);

            var result = await CreateSut().Handle(
                new GetMrsPendingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyMrsList_ReturnsEmptyData()
        {
            SetupDefaults(new List<MrsPendingDto>(), 0);

            var result = await CreateSut().Handle(
                new GetMrsPendingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetPendingMrsDetailsOnce()
        {
            SetupDefaults(new List<MrsPendingDto>(), 0);

            await CreateSut().Handle(
                new GetMrsPendingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetPendingMrsDetailsAsync(1, 10, null), Times.Once);
        }
    }
}
