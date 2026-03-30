using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.RackMaster.Queries
{
    public sealed class GetRackMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRackMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetRackMasterAutoCompleteQueryHanlder CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var dtos = RackMasterBuilders.ValidAutoCompleteList();
            _mockQueryRepo.Setup(r => r.GetRackMasterAutoCompletes(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GetRackMasterAutoCompleteDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetRackMasterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsResults()
        {
            var dtos = RackMasterBuilders.ValidAutoCompleteList();
            _mockQueryRepo.Setup(r => r.GetRackMasterAutoCompletes(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GetRackMasterAutoCompleteDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetRackMasterAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
