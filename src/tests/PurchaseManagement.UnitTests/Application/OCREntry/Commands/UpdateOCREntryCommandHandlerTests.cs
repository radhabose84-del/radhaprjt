using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Commands
{
    public sealed class UpdateOCREntryCommandHandlerTests
    {
        private readonly Mock<IOCREntryCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IOCREntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IOCREntryFileStorage> _mockFileStorage = new(MockBehavior.Loose);

        private UpdateOCREntryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockFileStorage.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.OCREntry>(It.IsAny<object>()))
                .Returns(OCREntryBuilders.ValidEntity(id));
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.OCREntry>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(id);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(OCREntryBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(OCREntryBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.OCREntry>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(OCREntryBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NewlyUploadedDocument_RenamedToOcrNumber()
        {
            var entity = OCREntryBuilders.ValidEntity(1);
            entity.DocumentPath = "TEMP_new.png"; // freshly uploaded temp file
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.OCREntry>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.GetByIdOCRWorkFlowAsync(1))
                .ReturnsAsync(new OCREntryWorkFlowDto { Id = 1, OcrNumber = "OCR-2026-0001" });
            _mockFileStorage
                .Setup(s => s.RenameAsync("TEMP_new.png", "OCR-2026-0001", It.IsAny<CancellationToken>()))
                .ReturnsAsync("OCR-2026-0001.png");
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.OCREntry>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(OCREntryBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.RenameAsync("TEMP_new.png", "OCR-2026-0001", It.IsAny<CancellationToken>()),
                Times.Once);
            entity.DocumentPath.Should().Be("OCR-2026-0001.png");
        }

        [Fact]
        public async Task Handle_ExistingDocument_NotRenamed()
        {
            var entity = OCREntryBuilders.ValidEntity(1);
            entity.DocumentPath = "OCR-2026-0001.png"; // already an OCR-number file, not a temp upload
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.OCREntry>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.OCREntry>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(OCREntryBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.RenameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
