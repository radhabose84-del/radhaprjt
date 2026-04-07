using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.PreventiveScheduler.Queries
{
    public sealed class GetPreventiveSchedulerByIdQueryHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPreventiveSchedulerByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            var entity = new PreventiveSchedulerHeader
            {
                Id = 1,
                MachineGroup = new MaintenanceManagement.Domain.Entities.MachineGroup(),
                MiscMaintenanceCategory = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                MiscSchedule = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                MiscFrequencyType = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                MiscFrequencyUnit = new MaintenanceManagement.Domain.Entities.MiscMaster()
            };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerHdrByIdDto>(It.IsAny<object>())).Returns(new PreventiveSchedulerHdrByIdDto { Activity = new(), PreventiveSchedulerDtl = new() });

            var result = await CreateSut().Handle(new GetPreventiveSchedulerByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = new PreventiveSchedulerHeader
            {
                Id = 1,
                MachineGroup = new MaintenanceManagement.Domain.Entities.MachineGroup(),
                MiscMaintenanceCategory = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                MiscSchedule = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                MiscFrequencyType = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                MiscFrequencyUnit = new MaintenanceManagement.Domain.Entities.MiscMaster()
            };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerHdrByIdDto>(It.IsAny<object>())).Returns(new PreventiveSchedulerHdrByIdDto { Activity = new(), PreventiveSchedulerDtl = new() });

            await CreateSut().Handle(new GetPreventiveSchedulerByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
