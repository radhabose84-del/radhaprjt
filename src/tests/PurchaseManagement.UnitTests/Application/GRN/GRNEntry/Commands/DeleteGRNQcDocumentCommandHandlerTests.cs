using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Commands
{
    public sealed class DeleteGRNQcDocumentCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileService = new(MockBehavior.Loose);
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private DeleteGRNQcDocumentCommandHandler CreateSut() =>
            new(_mockFileService.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsException()
        {
            var command = new DeleteGRNQcDocumentCommand { GrnQcdocumentPath = "test.pdf" };
            _mockRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync(string.Empty);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*Base directory not configured*");
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
