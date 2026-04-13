using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Queries
{
    public sealed class GetAllActivityMasterQueryHandlerBatchATests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetAllActivityMasterQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockRepo.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetAllActivityMasterAsync(1, 10, It.IsAny<string>()))
                .ReturnsAsync((new List<GetAllActivityMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllActivityMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PaginationMetadata_IsSet()
        {
            _mockRepo.Setup(r => r.GetAllActivityMasterAsync(3, 20, It.IsAny<string>()))
                .ReturnsAsync((new List<GetAllActivityMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllActivityMasterQuery { PageNumber = 3, PageSize = 20 },
                CancellationToken.None);

            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(20);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetAllActivityMasterAsync(1, 10, It.IsAny<string>()))
                .ReturnsAsync((new List<GetAllActivityMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllActivityMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockRepo.Verify(r => r.GetAllActivityMasterAsync(1, 10, It.IsAny<string>()), Times.Once);
        }
    }
}
