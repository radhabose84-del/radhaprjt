using AutoMapper;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityByMachinGroupId;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Queries
{
    public sealed class GetActivityByMachinGroupIdQueryHandlerBatchATests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetActivityByMachinGroupIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetActivityByMachinGroupId(5))
                .ReturnsAsync(new List<GetActivityByMachineGroupDto>());
            _mockMapper.Setup(m => m.Map<List<GetActivityByMachineGroupDto>>(It.IsAny<object>()))
                .Returns(new List<GetActivityByMachineGroupDto>());

            var result = await CreateSut().Handle(
                new GetActivityByMachinGroupIdQuery { MachineGroupId = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithMachineGroupId()
        {
            _mockQueryRepo.Setup(r => r.GetActivityByMachinGroupId(42))
                .ReturnsAsync(new List<GetActivityByMachineGroupDto>());
            _mockMapper.Setup(m => m.Map<List<GetActivityByMachineGroupDto>>(It.IsAny<object>()))
                .Returns(new List<GetActivityByMachineGroupDto>());

            await CreateSut().Handle(
                new GetActivityByMachinGroupIdQuery { MachineGroupId = 42 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetActivityByMachinGroupId(42), Times.Once);
        }
    }
}
