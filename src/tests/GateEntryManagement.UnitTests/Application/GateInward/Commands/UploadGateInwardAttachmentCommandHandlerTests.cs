using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment;
using Microsoft.AspNetCore.Http;

namespace GateEntryManagement.UnitTests.Application.GateInward.Commands
{
    public sealed class UploadGateInwardAttachmentCommandHandlerTests
    {
        private readonly Mock<IGateInwardAttachmentFileStorage> _mockStorage = new(MockBehavior.Strict);

        private UploadGateInwardAttachmentCommandHandler CreateSut() => new(_mockStorage.Object);

        [Fact]
        public async Task Handle_StagesFile_ReturnsResultDto()
        {
            var file = new Mock<IFormFile>().Object;
            _mockStorage
                .Setup(s => s.SaveToStagingAsync(file, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StagedGateInwardAttachment("TEMP_x.pdf"));

            var result = await CreateSut().Handle(
                new UploadGateInwardAttachmentCommand { File = file }, CancellationToken.None);

            result.FileName.Should().Be("TEMP_x.pdf");
        }

        [Fact]
        public async Task Handle_CallsStorageOnce()
        {
            var file = new Mock<IFormFile>().Object;
            _mockStorage
                .Setup(s => s.SaveToStagingAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StagedGateInwardAttachment("a"));

            await CreateSut().Handle(new UploadGateInwardAttachmentCommand { File = file }, CancellationToken.None);

            _mockStorage.Verify(s => s.SaveToStagingAsync(file, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
