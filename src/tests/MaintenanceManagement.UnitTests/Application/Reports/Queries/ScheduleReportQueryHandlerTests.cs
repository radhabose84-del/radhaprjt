using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.ScheduleReport;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class ScheduleReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private ScheduleReportQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockDeptLookup.Object);
        }

        private static ScheduleReportQuery ValidQuery() => new()
        {
            FromDueDate = DateTime.UtcNow.AddDays(-10),
            ToDueDate = DateTime.UtcNow
        };

        [Fact]
        public async Task Handle_WithEmptyResults_ReturnsNoData()
        {
            IEnumerable<dynamic> empty = new List<object>();
            _mockRepo.Setup(r => r.ScheduleReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(empty);
            _mockMapper.Setup(m => m.Map<List<ScheduleReportDto>>(It.IsAny<object>())).Returns(new List<ScheduleReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_NoMatchingDept_FiltersOut()
        {
            IEnumerable<dynamic> empty = new List<object>();
            var dtos = new List<ScheduleReportDto> { new() { DepartmentId = 999 } };
            _mockRepo.Setup(r => r.ScheduleReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(empty);
            _mockMapper.Setup(m => m.Map<List<ScheduleReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            IEnumerable<dynamic> empty = new List<object>();
            _mockRepo.Setup(r => r.ScheduleReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(empty);
            _mockMapper.Setup(m => m.Map<List<ScheduleReportDto>>(It.IsAny<object>())).Returns(new List<ScheduleReportDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.ScheduleReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Once);
        }
    }
}
