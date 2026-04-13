using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityCheckListMaster.Queries
{
    public sealed class GetAllActivityCheckListMasterQueryHandlerBatchATests
    {
        private readonly Mock<IActivityCheckListMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetAllActivityCheckListMasterQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllActivityCheckListMasterAsync(1, 10, null))
                .ReturnsAsync((new List<GetAllActivityCheckListMasterDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetAllActivityCheckListMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetAllActivityCheckListMasterDto>());

            var result = await CreateSut().Handle(
                new GetAllActivityCheckListMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PaginationMetadata_IsPreserved()
        {
            _mockRepo.Setup(r => r.GetAllActivityCheckListMasterAsync(2, 5, "term"))
                .ReturnsAsync((new List<GetAllActivityCheckListMasterDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetAllActivityCheckListMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetAllActivityCheckListMasterDto>());

            var result = await CreateSut().Handle(
                new GetAllActivityCheckListMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "term" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetAllActivityCheckListMasterAsync(1, 10, null))
                .ReturnsAsync((new List<GetAllActivityCheckListMasterDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetAllActivityCheckListMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetAllActivityCheckListMasterDto>());

            await CreateSut().Handle(
                new GetAllActivityCheckListMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockRepo.Verify(r => r.GetAllActivityCheckListMasterAsync(1, 10, null), Times.Once);
        }
    }
}
