using AutoMapper;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetMachineGroupById;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Queries
{
    public sealed class GetMachineGroupNameByIdQueryHandlerBatchATests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineGroupNameByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetMachineGroupById(5))
                .ReturnsAsync(new List<GetMachineGroupNameByIdDto>());
            _mockMapper.Setup(m => m.Map<List<GetMachineGroupNameByIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetMachineGroupNameByIdDto>());

            var result = await CreateSut().Handle(
                new GetMachineGroupNameByIdQuery { ActivityId = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithActivityId()
        {
            _mockQueryRepo.Setup(r => r.GetMachineGroupById(42))
                .ReturnsAsync(new List<GetMachineGroupNameByIdDto>());
            _mockMapper.Setup(m => m.Map<List<GetMachineGroupNameByIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetMachineGroupNameByIdDto>());

            await CreateSut().Handle(
                new GetMachineGroupNameByIdQuery { ActivityId = 42 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetMachineGroupById(42), Times.Once);
        }
    }
}
