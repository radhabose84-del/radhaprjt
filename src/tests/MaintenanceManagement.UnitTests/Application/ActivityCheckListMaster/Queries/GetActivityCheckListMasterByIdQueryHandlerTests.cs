using AutoMapper;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMasterById;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityCheckListMaster.Queries
{
    public sealed class GetActivityCheckListMasterByIdQueryHandlerTests
    {
        private readonly Mock<IActivityCheckListMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetActivityCheckListMasterByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new GetAllActivityCheckListMasterDto { ChecklistId = 1 });
            _mockMapper.Setup(m => m.Map<GetAllActivityCheckListMasterDto>(It.IsAny<object>())).Returns(new GetAllActivityCheckListMasterDto { ChecklistId = 1 });

            var result = await CreateSut().Handle(new GetActivityCheckListMasterByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }
}
