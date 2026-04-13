using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.MaterialPlanningReport;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class MaterialPlanningReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private MaterialPlanningReportQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockDeptLookup.Object);
        }

        private static MaterialPlanningReportQuery ValidQuery() => new()
        {
            FromDueDate = DateTime.UtcNow.AddDays(-10),
            ToDueDate = DateTime.UtcNow
        };

        [Fact]
        public async Task Handle_WithResults_ReturnsResponse()
        {
            IEnumerable<dynamic> empty = new List<object>();
            var dtos = new List<MaterialPlanningReportDto>();
            _mockRepo.Setup(r => r.MaterialPlanningReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(empty);
            _mockMapper.Setup(m => m.Map<List<MaterialPlanningReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            IEnumerable<dynamic> empty = new List<object>();
            _mockRepo.Setup(r => r.MaterialPlanningReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(empty);
            _mockMapper.Setup(m => m.Map<List<MaterialPlanningReportDto>>(It.IsAny<object>())).Returns(new List<MaterialPlanningReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            IEnumerable<dynamic> empty = new List<object>();
            _mockRepo.Setup(r => r.MaterialPlanningReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(empty);
            _mockMapper.Setup(m => m.Map<List<MaterialPlanningReportDto>>(It.IsAny<object>())).Returns(new List<MaterialPlanningReportDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.MaterialPlanningReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Once);
        }
    }
}
