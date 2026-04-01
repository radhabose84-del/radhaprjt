using System.ComponentModel.DataAnnotations;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Commands.UploadItemImage;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.UnitTests.Application.Item.Commands
{
    public sealed class UploadFileCommandHandlerTests
    {
        private readonly Mock<ILogger<UploadFileCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IItemQueryRepository> _mockItemRepo = new(MockBehavior.Strict);

        private UploadFileCommandHandler CreateSut() =>
            new(_mockLogger.Object, _mockItemRepo.Object);

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            Func<Task> act = async () => await CreateSut().Handle(
                new UploadFileCommand { File = null }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
