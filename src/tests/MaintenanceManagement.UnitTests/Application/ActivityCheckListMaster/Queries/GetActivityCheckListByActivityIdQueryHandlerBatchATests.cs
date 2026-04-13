using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetCheckListByActivityId;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityCheckListMaster.Queries
{
    public sealed class GetActivityCheckListByActivityIdQueryHandlerBatchATests
    {
        private readonly Mock<IActivityCheckListMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetActivityCheckListByActivityIdQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmpty()
        {
            _mockRepo.Setup(r => r.GetCheckListByActivityIdsAsync(It.IsAny<List<int>>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<GetActivityCheckListByActivityIdDto>());
            _mockMapper.Setup(m => m.Map<List<GetActivityCheckListByActivityIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetActivityCheckListByActivityIdDto>());

            var result = await CreateSut().Handle(
                new GetActivityCheckListByActivityIdQuery { Ids = new List<int>() },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_CallsRepository()
        {
            var dtos = new List<GetActivityCheckListByActivityIdDto>
            {
                new() { DepartmentId = 1, UnitId = 1 }
            };
            _mockRepo.Setup(r => r.GetCheckListByActivityIdsAsync(It.IsAny<List<int>>(), It.IsAny<int?>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GetActivityCheckListByActivityIdDto>>(It.IsAny<object>()))
                .Returns(dtos);

            await CreateSut().Handle(
                new GetActivityCheckListByActivityIdQuery { Ids = new List<int> { 1, 2 } },
                CancellationToken.None);

            _mockRepo.Verify(r => r.GetCheckListByActivityIdsAsync(It.IsAny<List<int>>(), It.IsAny<int?>()), Times.Once);
        }
    }
}
