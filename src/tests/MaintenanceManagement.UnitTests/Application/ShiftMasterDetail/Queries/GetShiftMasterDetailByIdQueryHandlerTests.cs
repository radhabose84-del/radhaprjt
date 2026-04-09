using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetailById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMasterDetail.Queries
{
    public sealed class GetShiftMasterDetailByIdQueryHandlerTests
    {
        private readonly Mock<IShiftMasterDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetShiftMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ShiftMasterDetail { Id = 1 };
            var dto = new ShiftMasterDetailByIdDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<ShiftMasterDetailByIdDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetShiftMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ShiftMasterDetail { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<ShiftMasterDetailByIdDto>(It.IsAny<object>())).Returns(new ShiftMasterDetailByIdDto());

            await CreateSut().Handle(new GetShiftMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
