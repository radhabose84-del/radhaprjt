using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IIssue;
using InventoryManagement.Application.Issue.Queries.GetPendingIssueHeader;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.Issue.Queries
{
    public sealed class GetPendingIssueHeaderQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWHLookup = new(MockBehavior.Loose);

        private GetPendingIssueHeaderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object, _mockWHLookup.Object);

        [Fact]
        public async Task Handle_ReturnsPaginatedResult()
        {
            var dtos = new List<GetPendingIssueHeaderDto> { new() };
            _mockQueryRepo
                .Setup(r => r.GetPendingIssueHeaderAsync(null, null, 1, 10, null))
                .ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<GetPendingIssueHeaderDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPendingIssueHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingIssueHeaderAsync(null, null, 1, 10, null))
                .ReturnsAsync((new List<GetPendingIssueHeaderDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetPendingIssueHeaderDto>>(It.IsAny<object>()))
                .Returns(new List<GetPendingIssueHeaderDto>());
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPendingIssueHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
