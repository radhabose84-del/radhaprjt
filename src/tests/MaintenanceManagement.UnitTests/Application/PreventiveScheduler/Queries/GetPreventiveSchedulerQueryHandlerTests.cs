using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveScheduler;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.PreventiveScheduler.Queries
{
    public sealed class GetPreventiveSchedulerQueryHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private PreventiveSchedulerQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<dynamic> { new { Id = 1 } };
            var dtos = new List<GetPreventiveSchedulerDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllPreventiveSchedulerAsync(1, 10, null, It.IsAny<List<int>>()))
                .ReturnsAsync(((IEnumerable<dynamic>)entities, 1));
            _mockMapper.Setup(m => m.Map<List<GetPreventiveSchedulerDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetPreventiveSchedulerQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllPreventiveSchedulerAsync(1, 10, null, It.IsAny<List<int>>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetPreventiveSchedulerDto>>(It.IsAny<object>())).Returns(new List<GetPreventiveSchedulerDto>());

            var result = await CreateSut().Handle(new GetPreventiveSchedulerQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetAllPreventiveSchedulerAsync(1, 10, null, It.IsAny<List<int>>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetPreventiveSchedulerDto>>(It.IsAny<object>())).Returns(new List<GetPreventiveSchedulerDto>());

            await CreateSut().Handle(new GetPreventiveSchedulerQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
