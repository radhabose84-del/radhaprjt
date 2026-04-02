using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Commands.DeleteItemImage;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.UnitTests.Application.Item.Commands
{
    public sealed class DeleteFileCommandHandlerTests
    {
        private readonly Mock<ILogger<DeleteFileCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IItemQueryRepository> _mockItemRepo = new(MockBehavior.Strict);
        private readonly Mock<IFileUploadService> _mockFileService = new(MockBehavior.Strict);

        private DeleteFileCommandHandler CreateSut() =>
            new(_mockLogger.Object, _mockItemRepo.Object, _mockFileService.Object);

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsException()
        {
            _mockItemRepo.Setup(r => r.GetBaseDirectoryAsync()).ReturnsAsync(string.Empty);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteFileCommand { imagePath = "test.jpg" }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Base directory not configured*");
        }

        [Fact]
        public async Task Handle_NullBaseDirectory_ThrowsException()
        {
            _mockItemRepo.Setup(r => r.GetBaseDirectoryAsync()).ReturnsAsync((string)null!);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteFileCommand { imagePath = "test.jpg" }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
