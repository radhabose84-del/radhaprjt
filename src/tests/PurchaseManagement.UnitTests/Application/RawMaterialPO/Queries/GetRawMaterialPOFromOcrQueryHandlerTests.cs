using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOFromOcr;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Queries
{
    public sealed class GetRawMaterialPOFromOcrQueryHandlerTests
    {
        private readonly Mock<IOCREntryQueryRepository> _mockOcrRepo = new(MockBehavior.Loose);
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRawMaterialPOFromOcrQueryHandler CreateSut() =>
            new(_mockOcrRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupOcr(decimal quantity = 800m) =>
            _mockOcrRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new OCREntryDto { Id = 1, OcrNumber = "OCR-2025-0004", Quantity = quantity });

        [Fact]
        public async Task Handle_NotConverted_ReturnsFullRemainingAndNotConverted()
        {
            SetupOcr(800m);
            _mockQueryRepo.Setup(r => r.GetConvertedQuantityAsync(1, null)).ReturnsAsync(0m);

            var result = await CreateSut().Handle(new GetRawMaterialPOFromOcrQuery { OcrId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.RemainingQuantity.Should().Be(800m);
            result.ConversionStatus.Should().Be(MiscEnumEntity.NotConverted);
        }

        [Fact]
        public async Task Handle_PartiallyConverted_ReturnsRemainingAndPartialStatus()
        {
            SetupOcr(800m);
            _mockQueryRepo.Setup(r => r.GetConvertedQuantityAsync(1, null)).ReturnsAsync(500m);

            var result = await CreateSut().Handle(new GetRawMaterialPOFromOcrQuery { OcrId = 1 }, CancellationToken.None);

            result!.ConvertedQuantity.Should().Be(500m);
            result.RemainingQuantity.Should().Be(300m);
            result.ConversionStatus.Should().Be(MiscEnumEntity.PartiallyConverted);
        }

        [Fact]
        public async Task Handle_FullyConverted_ReturnsZeroRemainingAndFullStatus()
        {
            SetupOcr(800m);
            _mockQueryRepo.Setup(r => r.GetConvertedQuantityAsync(1, null)).ReturnsAsync(800m);

            var result = await CreateSut().Handle(new GetRawMaterialPOFromOcrQuery { OcrId = 1 }, CancellationToken.None);

            result!.RemainingQuantity.Should().Be(0m);
            result.ConversionStatus.Should().Be(MiscEnumEntity.FullyConverted);
        }

        [Fact]
        public async Task Handle_OcrNotFound_ReturnsNull()
        {
            _mockOcrRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((OCREntryDto?)null);

            var result = await CreateSut().Handle(new GetRawMaterialPOFromOcrQuery { OcrId = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
