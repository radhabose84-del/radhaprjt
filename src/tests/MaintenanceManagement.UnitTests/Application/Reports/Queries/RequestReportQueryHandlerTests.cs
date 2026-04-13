using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.MaintenanceRequestReport;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class RequestReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private RequestReportQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        private static RequestReportQuery ValidQuery() => new()
        {
            RequestFromDate = DateTimeOffset.UtcNow.AddDays(-7),
            RequestToDate = DateTimeOffset.UtcNow,
            RequestType = 1,
            RequestStatus = 1,
            DepartmentId = 1
        };

        [Fact]
        public async Task Handle_WithEmptyResult_ReturnsNoData()
        {
            _mockRepo.Setup(r => r.MaintenanceReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<RequestReportDto>());
            _mockMapper.Setup(m => m.Map<List<RequestReportDto>>(It.IsAny<object>())).Returns(new List<RequestReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_NoMatchingDept_ReturnsNoData()
        {
            var dtos = new List<RequestReportDto> { new() { MaintenanceDepartmentId = 999 } };
            _mockRepo.Setup(r => r.MaintenanceReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<RequestReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.MaintenanceReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<RequestReportDto>());
            _mockMapper.Setup(m => m.Map<List<RequestReportDto>>(It.IsAny<object>())).Returns(new List<RequestReportDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.MaintenanceReportAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Once);
        }
    }
}
