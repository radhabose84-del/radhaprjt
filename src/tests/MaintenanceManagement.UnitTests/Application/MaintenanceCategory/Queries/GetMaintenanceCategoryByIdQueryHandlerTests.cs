using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryById;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceCategory.Queries
{
    public sealed class GetMaintenanceCategoryByIdQueryHandlerTests
    {
        private readonly Mock<IMaintenanceCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMaintenanceCategoryByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = MaintenanceCategoryBuilders.ValidEntity(1);
            var dto = MaintenanceCategoryBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<MaintenanceCategoryDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceCategoryByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = MaintenanceCategoryBuilders.ValidEntity(1);
            var dto = MaintenanceCategoryBuilders.ValidDto(1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<MaintenanceCategoryDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMaintenanceCategoryByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
