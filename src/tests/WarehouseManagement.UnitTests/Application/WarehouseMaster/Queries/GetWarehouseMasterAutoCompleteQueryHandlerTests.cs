using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Queries
{
    public sealed class GetWarehouseMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetWarehouseMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var dtos = WarehouseMasterBuilders.ValidAutoCompleteList();
            _mockQueryRepo.Setup(r => r.GetWarehouseMasterAutoCompletes(It.IsAny<string>())).ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GetWarehouseAutoCompleteDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetWarehouseMasterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var dtos = WarehouseMasterBuilders.ValidAutoCompleteList();
            _mockQueryRepo.Setup(r => r.GetWarehouseMasterAutoCompletes(It.IsAny<string>())).ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GetWarehouseAutoCompleteDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetWarehouseMasterAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
