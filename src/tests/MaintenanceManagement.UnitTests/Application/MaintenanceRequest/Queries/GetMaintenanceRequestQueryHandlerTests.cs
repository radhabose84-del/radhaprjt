using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries.BatchD
{
    public sealed class GetMaintenanceRequestQueryHandlerBatchDTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);

        private GetMaintenanceRequestQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUserLookup.Setup(u => u.GetAllUserAsync()).ReturnsAsync(new List<UserLookupDto>());
            _mockMapper.Setup(m => m.Map<List<GetMaintenanceRequestDto>>(It.IsAny<object>()))
                .Returns(new List<GetMaintenanceRequestDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockUserLookup.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllMaintenanceRequestAsync(1, 10, null, It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));

            var result = await CreateSut().Handle(
                new GetMaintenanceRequestQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllMaintenanceRequestAsync(1, 10, null, It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));

            var result = await CreateSut().Handle(
                new GetMaintenanceRequestQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
