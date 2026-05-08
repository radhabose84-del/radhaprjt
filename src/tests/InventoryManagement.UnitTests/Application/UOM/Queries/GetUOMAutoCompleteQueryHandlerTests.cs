using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMAutoComplete;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.UOM.Queries
{
    public sealed class GetUOMAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUOMAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingItems()
        {
            var entities = new List<InventoryManagement.Domain.Entities.UOM>
            {
                UOMBuilders.ValidEntity(1)
            };
            var dtos = new List<UOMAutoCompleteDto>
            {
                new UOMAutoCompleteDto { Id = 1, UOMName = "Kilogram" }
            };

            _mockQueryRepo
                .Setup(r => r.GetUOM("kg", null))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UOMAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = "kg" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetUOM(It.IsAny<string>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<InventoryManagement.Domain.Entities.UOM>());

            var result = await CreateSut().Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = "xyz" }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetUOM(It.IsAny<string>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<InventoryManagement.Domain.Entities.UOM>());

            await CreateSut().Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetUOM(It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ForwardsUOMTypeCode_ToRepository()
        {
            var entities = new List<InventoryManagement.Domain.Entities.UOM>
            {
                UOMBuilders.ValidEntity(1)
            };

            _mockQueryRepo
                .Setup(r => r.GetUOM("lt", "Volume Units"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UOMAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<UOMAutoCompleteDto> { new() { Id = 1, UOMName = "Litres" } });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = "lt", UOMTypeCode = "Volume Units" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockQueryRepo.Verify(r => r.GetUOM("lt", "Volume Units"), Times.Once);
        }
    }
}
