using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetDetailSchedulerByDate;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Queries.BatchD
{
    public sealed class GetDetailSchedulerByDateQueryHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetDetailSchedulerByDateQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMapper.Setup(m => m.Map<List<DetailSchedulerByDateDto>>(It.IsAny<object>()))
                .Returns(new List<DetailSchedulerByDateDto>());
            return new(_mockQuery.Object, _mockMapper.Object, _mockDeptLookup.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQuery.Setup(q => q.GetDetailSchedulerByDate(It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<dynamic>)new List<dynamic>());

            var result = await CreateSut().Handle(
                new GetDetailSchedulerByDateQuery { SchedulerDate = DateOnly.FromDateTime(DateTime.Today), DepartmentId = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsEmptyList()
        {
            _mockQuery.Setup(q => q.GetDetailSchedulerByDate(It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<dynamic>)new List<dynamic>());

            var result = await CreateSut().Handle(
                new GetDetailSchedulerByDateQuery { SchedulerDate = DateOnly.FromDateTime(DateTime.Today), DepartmentId = 1 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
        }
    }
}
