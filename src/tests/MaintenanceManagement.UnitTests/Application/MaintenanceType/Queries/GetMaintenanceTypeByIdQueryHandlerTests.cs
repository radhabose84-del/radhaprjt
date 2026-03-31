using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeById;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceType.Queries
{
    public sealed class GetMaintenanceTypeByIdQueryHandlerTests
    {
        private readonly Mock<IMaintenanceTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMaintenanceTypeByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = MaintenanceTypeBuilders.ValidEntity(1);
            var dto = MaintenanceTypeBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<MaintenanceTypeDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceTypeByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = MaintenanceTypeBuilders.ValidEntity(1);
            var dto = MaintenanceTypeBuilders.ValidDto(1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<MaintenanceTypeDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMaintenanceTypeByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
