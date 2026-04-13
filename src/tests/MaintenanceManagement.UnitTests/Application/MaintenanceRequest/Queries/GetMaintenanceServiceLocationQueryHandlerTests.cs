using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceServiceLocation;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries.BatchD
{
    public sealed class GetMaintenanceServiceLocationQueryHandlerBatchDTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetMaintenanceServiceLocationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetMaintenanceServiceLocationDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetMaintenanceServiceLocationQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithItems_ReturnsMappedList()
        {
            _mockQueryRepo.Setup(r => r.GetMaintenanceServiceLocationDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() { Id = 1 } });
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto> { new() { Id = 1 } });

            var result = await CreateSut().Handle(new GetMaintenanceServiceLocationQuery(), CancellationToken.None);

            result.Data.Count.Should().Be(1);
        }
    }
}
