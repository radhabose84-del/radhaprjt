using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryAutoComplete;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceCategory.Queries
{
    public sealed class GetMaintenanceCategoryAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMaintenanceCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMaintenanceCategoryAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MaintenanceCategory>
            {
                MaintenanceCategoryBuilders.ValidEntity()
            };
            var dtos = new List<MaintenanceCategoryAutoCompleteDto>
            {
                MaintenanceCategoryBuilders.ValidAutoCompleteDto()
            };

            _mockQueryRepo
                .Setup(r => r.GetMaintenanceCategoryAsync(It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceCategoryAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceCategoryAutoCompleteQuery { SearchPattern = "Elec" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMaintenanceCategoryAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MaintenanceCategory>());
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceCategoryAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceCategoryAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceCategoryAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
