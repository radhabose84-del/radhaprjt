using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMaster.Queries
{
    public sealed class GetShiftMasterByIdQueryHandlerTests
    {
        private readonly Mock<IShiftMasterQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetShiftMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ShiftMaster { Id = 1, ShiftCode = "S1", ShiftName = "Morning" };
            var dto = new ShiftMasterDTO { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<ShiftMasterDTO>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetShiftMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ShiftMaster { Id = 1, ShiftCode = "S1", ShiftName = "Morning" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<ShiftMasterDTO>(It.IsAny<object>())).Returns(new ShiftMasterDTO { Id = 1 });

            await CreateSut().Handle(new GetShiftMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
