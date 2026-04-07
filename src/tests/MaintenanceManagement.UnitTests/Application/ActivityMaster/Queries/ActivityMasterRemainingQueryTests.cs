using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityType;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Queries
{
    public sealed class GetAllActivityMasterQueryHandlerTests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetAllActivityMasterQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockRepo.Object, _mockMediator.Object, _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetAllActivityMasterAsync(1, 10, It.IsAny<string>()))
                .ReturnsAsync((new List<GetAllActivityMasterDto>(), 0));

            var result = await CreateSut().Handle(new GetAllActivityMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }

    public sealed class GetActivityTypeQueryHandlerTests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetActivityTypeQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetActivityTypeAsync()).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>())).Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetActivityTypeQuery(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
