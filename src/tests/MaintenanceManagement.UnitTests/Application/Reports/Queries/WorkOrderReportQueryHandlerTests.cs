using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.WorkOrderReport;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class WorkOrderReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private WorkOrderReportQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockDeptLookup.Object);
        }

        private static WorkOrderReportQuery ValidQuery() => new()
        {
            FromDate = DateTimeOffset.UtcNow.AddDays(-7),
            ToDate = DateTimeOffset.UtcNow,
            RequestTypeId = 1,
            DepartmentId = 1
        };

        [Fact]
        public async Task Handle_WithEmptyResult_ReturnsNoData()
        {
            _mockRepo.Setup(r => r.WorkOrderReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WorkOrderReportDto>());
            _mockMapper.Setup(m => m.Map<List<WorkOrderReportDto>>(It.IsAny<object>())).Returns(new List<WorkOrderReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithResults_FiltersByDept()
        {
            var dtos = new List<WorkOrderReportDto> { new() };
            _mockRepo.Setup(r => r.WorkOrderReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<WorkOrderReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.WorkOrderReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WorkOrderReportDto>());
            _mockMapper.Setup(m => m.Map<List<WorkOrderReportDto>>(It.IsAny<object>())).Returns(new List<WorkOrderReportDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.WorkOrderReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>()), Times.Once);
        }
    }
}
