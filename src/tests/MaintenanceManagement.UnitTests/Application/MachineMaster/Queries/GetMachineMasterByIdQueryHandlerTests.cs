using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Queries
{
    public sealed class GetMachineMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineMasterByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MachineMasterDto { Id = 1 });
            _mockMapper.Setup(m => m.Map<MachineMasterDto>(It.IsAny<object>())).Returns(new MachineMasterDto { Id = 1 });

            var result = await CreateSut().Handle(new GetMachineMasterByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }

    public sealed class GetMachineMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetMachineMasterAutoCompleteQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto> { new() { DepartmentId = 0 } });
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetMachineAsync(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineMaster> { new() });
            _mockMapper.Setup(m => m.Map<List<MachineMasterAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<MachineMasterAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetMachineMasterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
