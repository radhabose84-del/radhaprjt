using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveScheduler;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Queries.BatchD
{
    public sealed class PreventiveSchedulerQueryHandlerBatchDTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private PreventiveSchedulerQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMapper.Setup(m => m.Map<List<GetPreventiveSchedulerDto>>(It.IsAny<object>()))
                .Returns(new List<GetPreventiveSchedulerDto>());
            return new(_mockQuery.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQuery.Setup(q => q.GetAllPreventiveSchedulerAsync(1, 10, null, It.IsAny<List<int>>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));

            var result = await CreateSut().Handle(
                new GetPreventiveSchedulerQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQuery.Setup(q => q.GetAllPreventiveSchedulerAsync(1, 10, null, It.IsAny<List<int>>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));

            await CreateSut().Handle(
                new GetPreventiveSchedulerQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
