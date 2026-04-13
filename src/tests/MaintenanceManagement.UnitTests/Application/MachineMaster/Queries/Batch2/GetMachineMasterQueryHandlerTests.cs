using AutoMapper;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Queries.Batch2
{
    public sealed class GetMachineMasterQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IAssetSpecificationLookup> _mockAssetLookup = new(MockBehavior.Loose);

        private GetMachineMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockAssetLookup.Object);

        private void SetupHappyPath(List<MachineMasterDto>? repoDtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMachineAsync(It.IsAny<string?>()))
                .ReturnsAsync(repoDtos ?? new List<MachineMasterDto>());
            _mockMapper
                .Setup(m => m.Map<List<MachineMasterDto>>(It.IsAny<object>()))
                .Returns<object>(src => (List<MachineMasterDto>)src);
            _mockDeptLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>());
            _mockAssetLookup
                .Setup(a => a.GetAllAssetSpecificationAsync())
                .ReturnsAsync(new List<AssetSpecificationLookupDto>());
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResponse()
        {
            SetupHappyPath(new List<MachineMasterDto> { new() { Id = 1, DepartmentId = 1, AssetId = 10 } });

            var result = await CreateSut().Handle(
                new GetMachineMasterQuery { SearchTerm = "x" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_PopulatesDepartmentNameFromLookup()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMachineAsync(It.IsAny<string?>()))
                .ReturnsAsync(new List<MachineMasterDto> { new() { Id = 1, DepartmentId = 1, AssetId = 10 } });
            _mockMapper
                .Setup(m => m.Map<List<MachineMasterDto>>(It.IsAny<object>()))
                .Returns<object>(src => (List<MachineMasterDto>)src);
            _mockDeptLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new() { DepartmentId = 1, DepartmentName = "Prod" }
                });
            _mockAssetLookup
                .Setup(a => a.GetAllAssetSpecificationAsync())
                .ReturnsAsync(new List<AssetSpecificationLookupDto>
                {
                    new() { AssetId = 10, SpecificationName = "Make", SpecificationValue = "BSOFT" }
                });

            var result = await CreateSut().Handle(
                new GetMachineMasterQuery { SearchTerm = "x" },
                CancellationToken.None);

            result.Data![0].ProductionDepartmentName.Should().Be("Prod");
            result.Data![0].SpecificationName.Should().Be("BSOFT");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMachineMasterQuery { SearchTerm = "x" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineMaster"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
