using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMasterDocument;
using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Application.PartyMaster.Commands
{
    public sealed class DeletePartyMasterDocumentCommandHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Strict);
        private readonly Mock<IPartyMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPartyActivityLogCommandRepository> _mockActivityLog = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private DeletePartyMasterDocumentCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockFileUpload.Object, _mockCommandRepo.Object,
                _mockActivityLog.Object, _mockIpService.Object);

        private void SetupIpDefaults()
        {
            _mockIpService.Setup(x => x.GetUserId()).Returns(1);
            _mockIpService.Setup(x => x.GetUserName()).Returns("test-user");
            _mockIpService.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsException()
        {
            _mockQueryRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync(string.Empty);

            var command = new DeletePartyMasterDocumentCommand
            {
                partydocumentPath = "test.pdf",
                Id = 1,
                PartyId = 1,
                FileName = "test.pdf"
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*Base directory not configured*");
        }

        [Fact]
        public async Task Handle_NullBaseDirectory_ThrowsException()
        {
            _mockQueryRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync((string?)null!);

            var command = new DeletePartyMasterDocumentCommand
            {
                partydocumentPath = "test.pdf",
                Id = 1,
                PartyId = 1,
                FileName = "test.pdf"
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*Base directory not configured*");
        }

        [Fact]
        public async Task Handle_FileNotExists_ThrowsFileNotFoundException()
        {
            _mockQueryRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync("PartyDocs");

            var command = new DeletePartyMasterDocumentCommand
            {
                partydocumentPath = "nonexistent_file_12345.pdf",
                Id = 1,
                PartyId = 1,
                FileName = "nonexistent_file_12345.pdf"
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FileNotFoundException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task Handle_ZeroIdAndPartyId_SkipsDbDeleteAndActivityLog()
        {
            // When Id = 0 and PartyId = 0, the handler should skip DB delete + activity log
            // but still attempt file delete (which will fail for nonexistent file)
            _mockQueryRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync("PartyDocs");

            var command = new DeletePartyMasterDocumentCommand
            {
                partydocumentPath = "nonexistent.pdf",
                Id = 0,
                PartyId = 0,
                FileName = "nonexistent.pdf"
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            // Should throw FileNotFoundException (file doesn't exist), NOT a DB or activity log error
            await act.Should().ThrowAsync<FileNotFoundException>();

            _mockCommandRepo.Verify(
                r => r.DeleteFileDetailsDocumentAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
