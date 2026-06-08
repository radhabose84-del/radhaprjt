using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Commands
{
    public sealed class UpdateRawMaterialPOCommandHandlerTests
    {
        private readonly Mock<IRawMaterialPOCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IRawMaterialPOFileStorage> _mockFileStorage = new(MockBehavior.Loose);

        private UpdateRawMaterialPOCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object,
                _mockMapper.Object, _mockMisc.Object, _mockFileStorage.Object);

        private void SetupHappyPath(int id = 1, decimal ocrQuantity = 800m, decimal otherConverted = 0m)
        {
            _mockQueryRepo.Setup(q => q.GetByIdAsync(id)).ReturnsAsync(RawMaterialPOBuilders.ValidDto(id));
            _mockMapper
                .Setup(m => m.Map<RawMaterialPOHeader>(It.IsAny<object>()))
                .Returns(RawMaterialPOBuilders.ValidEntity(id));
            _mockQueryRepo.Setup(q => q.GetOcrQuantityAsync(It.IsAny<int>())).ReturnsAsync(ocrQuantity);
            _mockQueryRepo.Setup(q => q.GetConvertedQuantityAsync(It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(otherConverted);
            _mockMisc
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 9 });
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<RawMaterialPOHeader>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(id);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(RawMaterialPOBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(RawMaterialPOBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<RawMaterialPOHeader>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(RawMaterialPOBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FullConversion_SetsFullyConvertedStatus()
        {
            // 600 requested + 300 other = 900 >= 800 → Fully Converted
            SetupHappyPath(ocrQuantity: 800m, otherConverted: 300m);
            await CreateSut().Handle(RawMaterialPOBuilders.ValidUpdateCommand(quantity: 600m), CancellationToken.None);

            _mockMisc.Verify(
                m => m.GetMiscMasterByName(MiscEnumEntity.ConversionStatus, MiscEnumEntity.FullyConverted),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithTempDocument_RenamesToPONumber()
        {
            RawMaterialPOHeader? captured = null;
            SetupHappyPath();

            var entity = RawMaterialPOBuilders.ValidEntity();
            entity.DocumentPath = "TEMP_xyz.png";
            _mockMapper.Setup(m => m.Map<RawMaterialPOHeader>(It.IsAny<object>())).Returns(entity);

            _mockFileStorage
                .Setup(s => s.RenameAsync("TEMP_xyz.png", "RMPO-2026-0001", It.IsAny<CancellationToken>()))
                .ReturnsAsync("RMPO-2026-0001.png");
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<RawMaterialPOHeader>(), It.IsAny<CancellationToken>()))
                .Callback<RawMaterialPOHeader, CancellationToken>((e, _) => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(RawMaterialPOBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.RenameAsync("TEMP_xyz.png", "RMPO-2026-0001", It.IsAny<CancellationToken>()),
                Times.Once);
            captured!.DocumentPath.Should().Be("RMPO-2026-0001.png");
        }

        [Fact]
        public async Task Handle_WithAlreadyAttachedDocument_DoesNotRename()
        {
            SetupHappyPath();

            var entity = RawMaterialPOBuilders.ValidEntity();
            entity.DocumentPath = "RMPO-2026-0001.png";
            _mockMapper.Setup(m => m.Map<RawMaterialPOHeader>(It.IsAny<object>())).Returns(entity);

            await CreateSut().Handle(RawMaterialPOBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.RenameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
