using AutoMapper;
using FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubCategories.Commands
{
    public sealed class DeleteAssetSubCategoriesCommandHandlerTests
    {
        private readonly Mock<IAssetSubCategoriesCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetSubCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAssetSubCategoriesCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = AssetSubCategoriesBuilders.ValidDto(id);
            var entity = AssetSubCategoriesBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockQueryRepo
                .Setup(r => r.IsAssetSubCategoryLinkedAsync(id))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetSubCategories>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<FAM.Domain.Entities.AssetSubCategories>()))
                .ReturnsAsync(id);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsId()
        {
            SetupHappyPath(1);
            var command = AssetSubCategoriesBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(1);
            var command = AssetSubCategoriesBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetSubCategories>()), Times.Once);
        }
    }
}
