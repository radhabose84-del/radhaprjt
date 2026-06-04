using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryById;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Queries
{
    public sealed class GetOCREntryByIdQueryHandlerTests
    {
        private const string BaseUrl = "http://192.168.1.126/Resources/";

        private readonly Mock<IOCREntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IOCREntryFileStorage> _mockFileStorage = new(MockBehavior.Loose);

        private GetOCREntryByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMisc.Object, _mockFileStorage.Object);

        private void SetupPathDependencies(string fullUrl)
        {
            _mockMisc
                .Setup(m => m.GetMiscTypeDescriptionAsync("ImagePath"))
                .ReturnsAsync(BaseUrl);
            _mockFileStorage
                .Setup(s => s.BuildPublicUrlAsync(BaseUrl, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fullUrl);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((OCREntryDto?)null);

            var result = await CreateSut().Handle(new GetOCREntryByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithDocument_BuildsFullUrlFromOcrPathMisc()
        {
            var dto = OCREntryBuilders.ValidDto(1);
            dto.DocumentPath = "OCR-2026-0001.png";
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            SetupPathDependencies("http://192.168.1.126/Resources/Purchase/OCRDocuments/Acme/Unit1/OCR-2026-0001.png");

            var result = await CreateSut().Handle(new GetOCREntryByIdQuery { Id = 1 }, CancellationToken.None);

            result!.DocumentFullPath.Should().Be(
                "http://192.168.1.126/Resources/Purchase/OCRDocuments/Acme/Unit1/OCR-2026-0001.png");
            _mockFileStorage.Verify(
                s => s.BuildPublicUrlAsync(BaseUrl, "OCR-2026-0001.png", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoDocument_LeavesFullPathNull()
        {
            var dto = OCREntryBuilders.ValidDto(1);
            dto.DocumentPath = null;
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetOCREntryByIdQuery { Id = 1 }, CancellationToken.None);

            result!.DocumentFullPath.Should().BeNull();
            _mockFileStorage.Verify(
                s => s.BuildPublicUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithDocument_PublishesAuditEvent()
        {
            var dto = OCREntryBuilders.ValidDto(1);
            dto.DocumentPath = "OCR-2026-0001.png";
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            SetupPathDependencies("http://x/y.png");

            await CreateSut().Handle(new GetOCREntryByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
