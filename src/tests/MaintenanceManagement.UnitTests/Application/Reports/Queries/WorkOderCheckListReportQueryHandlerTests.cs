using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.WorkOderCheckListReport;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class WorkOderCheckListReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private WorkOderCheckListReportQueryHandler CreateSut()
        {
            _mockIpService.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockIpService.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object);
        }

        private static WorkOderCheckListReportQuery ValidQuery() => new()
        {
            WorkOrderFromDate = DateTimeOffset.UtcNow.AddDays(-7),
            WorkOrderToDate = DateTimeOffset.UtcNow,
            UnitId = 1,
            MachineGroupId = 1,
            MachineId = 1,
            ActivityId = 1
        };

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<WorkOderCheckListReportDto> { new() };
            _mockRepo.Setup(r => r.GetWorkOrderChecklistReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<WorkOderCheckListReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsNoData()
        {
            _mockRepo.Setup(r => r.GetWorkOrderChecklistReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WorkOderCheckListReportDto>());
            _mockMapper.Setup(m => m.Map<List<WorkOderCheckListReportDto>>(It.IsAny<object>())).Returns(new List<WorkOderCheckListReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetWorkOrderChecklistReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<WorkOderCheckListReportDto>());
            _mockMapper.Setup(m => m.Map<List<WorkOderCheckListReportDto>>(It.IsAny<object>())).Returns(new List<WorkOderCheckListReportDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetWorkOrderChecklistReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Once);
        }
    }
}
