using AutoMapper;
using FAM.Application.AssetCategories.Command.DeleteAssetCategories;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetCategories.Commands
{
    public sealed class DeleteAssetCategoriesCommandHandlerTests
    {
        private readonly Mock<IAssetCategoriesCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAssetCategoriesCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = AssetCategoriesBuilders.ValidDto(id);
            var entity = AssetCategoriesBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockQueryRepo
                .Setup(r => r.IsAssetCategoryLinkedAsync(id))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetCategories>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<FAM.Domain.Entities.AssetCategories>()))
                .ReturnsAsync(id);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsId()
        {
            SetupHappyPath(1);
            var command = AssetCategoriesBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(1);
            var command = AssetCategoriesBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetCategories>()), Times.Once);
        }
    }
}
