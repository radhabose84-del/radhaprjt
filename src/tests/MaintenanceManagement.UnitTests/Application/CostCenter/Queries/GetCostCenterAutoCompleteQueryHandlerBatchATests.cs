using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterAutoComplete;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.CostCenter.Queries
{
    public sealed class GetCostCenterAutoCompleteQueryHandlerBatchATests
    {
        private readonly Mock<ICostCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCostCenterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetCostCenterGroups(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.CostCenter>());
            _mockMapper.Setup(m => m.Map<List<CostCenterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<CostCenterAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetCostCenterAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetCostCenterGroups("pat"))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.CostCenter>());
            _mockMapper.Setup(m => m.Map<List<CostCenterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<CostCenterAutoCompleteDto>());

            await CreateSut().Handle(
                new GetCostCenterAutoCompleteQuery { SearchPattern = "pat" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetCostCenterGroups("pat"), Times.Once);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.CostCenter> { new() };
            var dtos = new List<CostCenterAutoCompleteDto> { new() };

            _mockQueryRepo.Setup(r => r.GetCostCenterGroups(It.IsAny<string>())).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<CostCenterAutoCompleteDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetCostCenterAutoCompleteQuery { SearchPattern = "x" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
