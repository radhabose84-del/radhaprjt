using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.UnitTests.Application.Templates.Commands
{
    public sealed class DeleteTemplateCommandHandlerTests
    {
        private readonly Mock<ITemplateCommandRepository> _mockCmd = new(MockBehavior.Strict);
        private readonly Mock<ITemplateQueryRepository> _mockQry = new(MockBehavior.Strict);

        private DeleteTemplateCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockQry.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            _mockQry.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InspectionTemplate { Id = 1, TemplateName = "T1" });
            _mockCmd.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteTemplateCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFalse()
        {
            _mockQry.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InspectionTemplate?)null);

            var result = await CreateSut().Handle(new DeleteTemplateCommand { Id = 99 }, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExistingId_CallsSoftDelete()
        {
            _mockQry.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InspectionTemplate { Id = 1, TemplateName = "T1" });
            _mockCmd.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteTemplateCommand { Id = 1 }, CancellationToken.None);

            _mockCmd.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
