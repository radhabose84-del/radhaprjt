using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeAutoComplete;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceType.Queries
{
    public sealed class GetMaintenanceTypeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMaintenanceTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMaintenanceTypeAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MaintenanceType>
            {
                MaintenanceTypeBuilders.ValidEntity()
            };
            var dtos = new List<MaintenanceTypeAutoCompleteDto>
            {
                MaintenanceTypeBuilders.ValidAutoCompleteDto()
            };

            _mockQueryRepo
                .Setup(r => r.GetMaintenanceTypeAsync(It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceTypeAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceTypeAutoCompleteQuery { SearchPattern = "Prev" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMaintenanceTypeAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MaintenanceType>());
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceTypeAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceTypeAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceTypeAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
