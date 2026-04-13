using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Queries.GetWorkOrderTests
{
    public sealed class GetWorkOrderQueryHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetWorkOrderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);

        private void SetupHappyPath(List<WorkOrderWithScheduleDto>? woList = null, List<GetWorkOrderDto>? mapped = null)
        {
            _mockQueryRepo.Setup(r => r.GetAllWOAsync(
                    It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(woList ?? new List<WorkOrderWithScheduleDto>());

            _mockMapper.Setup(m => m.Map<List<GetWorkOrderDto>>(It.IsAny<object>()))
                .Returns(mapped ?? new List<GetWorkOrderDto>());

            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new() { DepartmentId = 1, DepartmentName = "Dept1" }
                });
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsSuccess()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(new GetWorkOrderQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(new GetWorkOrderQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllWOAsync(
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDepartmentMatch_ReturnsGroupedData()
        {
            var mapped = new List<GetWorkOrderDto>
            {
                new() { DepartmentId = 1, MaintenanceType = "TypeA" },
                new() { DepartmentId = 1, MaintenanceType = "TypeB" }
            };
            SetupHappyPath(new List<WorkOrderWithScheduleDto> { new(), new() }, mapped);

            var result = await CreateSut().Handle(new GetWorkOrderQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(new GetWorkOrderQuery(), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.IsAny<MaintenanceManagement.Domain.Events.AuditLogsDomainEvent>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
