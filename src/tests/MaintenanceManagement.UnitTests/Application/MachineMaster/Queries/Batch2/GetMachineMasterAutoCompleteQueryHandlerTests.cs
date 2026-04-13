using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Queries.Batch2
{
    public sealed class GetMachineMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetMachineMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_FiltersToOnlyValidDepartments()
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineMaster>());
            _mockMapper
                .Setup(m => m.Map<List<MachineMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<MachineMasterAutoCompleteDto>
                {
                    new() { Id = 1, DepartmentId = 1 },
                    new() { Id = 2, DepartmentId = 99 } // invalid
                });
            _mockDeptLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new() { DepartmentId = 1 }
                });

            var result = await CreateSut().Handle(
                new GetMachineMasterAutoCompleteQuery { SearchPattern = "x" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].DepartmentId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineMaster>());
            _mockMapper
                .Setup(m => m.Map<List<MachineMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<MachineMasterAutoCompleteDto>());
            _mockDeptLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>());

            var result = await CreateSut().Handle(
                new GetMachineMasterAutoCompleteQuery { SearchPattern = "x" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineMaster>());
            _mockMapper
                .Setup(m => m.Map<List<MachineMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<MachineMasterAutoCompleteDto>());
            _mockDeptLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>());

            await CreateSut().Handle(
                new GetMachineMasterAutoCompleteQuery { SearchPattern = "x" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineMaster"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
