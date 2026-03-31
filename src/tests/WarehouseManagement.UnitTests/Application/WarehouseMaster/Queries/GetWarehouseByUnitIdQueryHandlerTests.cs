using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWarehouseByUnitId;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Queries
{
    public sealed class GetWarehouseByUnitIdQueryHandlerTests
    {
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetWarehouseByUnitIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsWarehouseListForUnit()
        {
            var dtos = WarehouseMasterBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetByUnitIdAsync(1))
                .ReturnsAsync(dtos);
            _mockMapper
                .Setup(m => m.Map<List<GetWarehouseAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetWarehouseByUnitIdQuery { UnitId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var empty = new List<GetWarehouseAutoCompleteDto>();

            _mockQueryRepo
                .Setup(r => r.GetByUnitIdAsync(99))
                .ReturnsAsync(empty);
            _mockMapper
                .Setup(m => m.Map<List<GetWarehouseAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(empty);

            var result = await CreateSut().Handle(
                new GetWarehouseByUnitIdQuery { UnitId = 99 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithCorrectUnitId()
        {
            var dtos = WarehouseMasterBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetByUnitIdAsync(5))
                .ReturnsAsync(dtos);
            _mockMapper
                .Setup(m => m.Map<List<GetWarehouseAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            await CreateSut().Handle(
                new GetWarehouseByUnitIdQuery { UnitId = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByUnitIdAsync(5), Times.Once);
        }
    }
}
