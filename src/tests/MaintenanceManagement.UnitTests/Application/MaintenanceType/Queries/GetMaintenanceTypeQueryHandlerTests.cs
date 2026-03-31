using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceType.Queries
{
    public sealed class GetMaintenanceTypeQueryHandlerTests
    {
        private readonly Mock<IMaintenanceTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMaintenanceTypeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MaintenanceType>
            {
                MaintenanceTypeBuilders.ValidEntity()
            };
            var dtos = new List<MaintenanceTypeDto> { MaintenanceTypeBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMaintenanceTypeAsync(1, 10, null))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceTypeDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceTypeQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMaintenanceTypeAsync(1, 10, null))
                .ReturnsAsync((new List<MaintenanceManagement.Domain.Entities.MaintenanceType>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceTypeDto>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceTypeDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceTypeQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
