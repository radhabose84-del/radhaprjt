using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceCategory.Queries
{
    public sealed class GetMaintenanceCategoryQueryHandlerTests
    {
        private readonly Mock<IMaintenanceCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMaintenanceCategoryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MaintenanceCategory>
            {
                MaintenanceCategoryBuilders.ValidEntity()
            };
            var dtos = new List<MaintenanceCategoryDto> { MaintenanceCategoryBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMaintenanceCategoryAsync(1, 10, null))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceCategoryDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceCategoryQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsPaginationMetadata()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MaintenanceCategory>
            {
                MaintenanceCategoryBuilders.ValidEntity()
            };
            var dtos = new List<MaintenanceCategoryDto> { MaintenanceCategoryBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMaintenanceCategoryAsync(2, 5, "elec"))
                .ReturnsAsync((entities, 11));
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceCategoryDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceCategoryQuery { PageNumber = 2, PageSize = 5, SearchTerm = "elec" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMaintenanceCategoryAsync(1, 10, null))
                .ReturnsAsync((new List<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceCategoryDto>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceCategoryDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMaintenanceCategoryQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
