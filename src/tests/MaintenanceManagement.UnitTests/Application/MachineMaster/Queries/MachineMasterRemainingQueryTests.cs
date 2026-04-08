using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineLineNo;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Queries
{
    public sealed class GetMachineMasterGetAllQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IAssetSpecificationLookup> _mockAssetLookup = new(MockBehavior.Loose);

        private GetMachineMasterQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockAssetLookup.Setup(a => a.GetAllAssetSpecificationAsync()).ReturnsAsync(new List<Contracts.Dtos.Lookups.FixedAssetManagement.AssetSpecificationLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object, _mockAssetLookup.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetAllMachineAsync(It.IsAny<string>())).ReturnsAsync(new List<MachineMasterDto>());
            _mockMapper.Setup(m => m.Map<List<MachineMasterDto>>(It.IsAny<object>())).Returns(new List<MachineMasterDto>());

            var result = await CreateSut().Handle(new GetMachineMasterQuery(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }

    public sealed class GetMachineLinenoQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineLinenoQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetMachineLineNoAsync()).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>())).Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetMachineLinenoQuery(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
