using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetParentWarehouseMaster;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Queries
{
    public sealed class GetParentWarehouseMasterQueryHandlerTests
    {
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetParentWarehouseMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsParentWarehouseList()
        {
            var dtos = new List<GetParentWarehouseDto>
            {
                new GetParentWarehouseDto { Id = 1, ParentWarehouseCode = "WH001", ParentWarehouseName = "Main Warehouse" }
            };

            _mockQueryRepo
                .Setup(r => r.GetParentWarehouseMaster())
                .ReturnsAsync(dtos);
            _mockMapper
                .Setup(m => m.Map<List<GetParentWarehouseDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(new GetParentWarehouseMasterQuery(), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ParentWarehouseCode.Should().Be("WH001");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var empty = new List<GetParentWarehouseDto>();

            _mockQueryRepo
                .Setup(r => r.GetParentWarehouseMaster())
                .ReturnsAsync(empty);
            _mockMapper
                .Setup(m => m.Map<List<GetParentWarehouseDto>>(It.IsAny<object>()))
                .Returns(empty);

            var result = await CreateSut().Handle(new GetParentWarehouseMasterQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var dtos = new List<GetParentWarehouseDto>();

            _mockQueryRepo
                .Setup(r => r.GetParentWarehouseMaster())
                .ReturnsAsync(dtos);
            _mockMapper
                .Setup(m => m.Map<List<GetParentWarehouseDto>>(It.IsAny<object>()))
                .Returns(dtos);

            await CreateSut().Handle(new GetParentWarehouseMasterQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetParentWarehouseMaster(), Times.Once);
        }
    }
}
