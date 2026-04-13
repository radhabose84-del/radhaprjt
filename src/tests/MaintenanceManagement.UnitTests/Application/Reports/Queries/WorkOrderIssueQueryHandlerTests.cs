using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.WorkOrderItemConsuption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class WorkOrderIssueQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private WorkOrderIssueQueryHandler CreateSut()
        {
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockUnitLookup.Object, _mockDeptLookup.Object);
        }

        private static WorkOrderIssueQuery ValidQuery() => new()
        {
            IssueFrom = DateTimeOffset.UtcNow.AddDays(-7),
            IssueTo = DateTimeOffset.UtcNow
        };

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<WorkOrderIssueDto> { new() };
            _mockRepo.Setup(r => r.GetItemConsumptionAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<WorkOrderIssueDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetItemConsumptionAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<WorkOrderIssueDto>());
            _mockMapper.Setup(m => m.Map<List<WorkOrderIssueDto>>(It.IsAny<object>())).Returns(new List<WorkOrderIssueDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullFromDate_ThrowsArgumentNullException()
        {
            var query = new WorkOrderIssueQuery { IssueFrom = null, IssueTo = DateTimeOffset.UtcNow };

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetItemConsumptionAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<WorkOrderIssueDto>());
            _mockMapper.Setup(m => m.Map<List<WorkOrderIssueDto>>(It.IsAny<object>())).Returns(new List<WorkOrderIssueDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetItemConsumptionAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once);
        }
    }
}
