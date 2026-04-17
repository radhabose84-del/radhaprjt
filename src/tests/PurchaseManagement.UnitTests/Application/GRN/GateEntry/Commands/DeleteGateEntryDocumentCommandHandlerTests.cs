using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Commands.DeleteGateEntryDocument;

namespace PurchaseManagement.UnitTests.Application.GRN.GateEntry.Commands
{
    public sealed class DeleteGateEntryDocumentCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileService = new(MockBehavior.Loose);
        private readonly Mock<IGateEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private DeleteGateEntryDocumentCommandHandler CreateSut() =>
            new(_mockFileService.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsException()
        {
            var command = new DeleteGateEntryDocumentCommand { GateEntrydocumentPath = "test.pdf" };
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
