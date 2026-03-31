using AutoMapper;
using MediatR;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.BinMaster.Queries
{
    public sealed class GetBinMasterAutoCompleteHandlerTests
    {
        private readonly Mock<IBinMasterQueryRepository> _mockBinRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetBinMasterAutoCompleteHandler CreateSut() =>
            new(_mockBinRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var dtos = BinMasterBuilders.ValidAutoCompleteList();
            _mockBinRepo.Setup(r => r.AutocompleteAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await CreateSut().Handle(
                new GetBinMasterAutoComplete { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsResults()
        {
            var dtos = BinMasterBuilders.ValidAutoCompleteList();
            _mockBinRepo.Setup(r => r.AutocompleteAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await CreateSut().Handle(
                new GetBinMasterAutoComplete { SearchPattern = null }, CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
