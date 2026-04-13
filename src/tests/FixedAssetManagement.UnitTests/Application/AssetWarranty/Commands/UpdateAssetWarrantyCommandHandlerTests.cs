using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UpdateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Commands
{
    public sealed class UpdateAssetWarrantyCommandHandlerTests
    {
        private readonly Mock<IAssetWarrantyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetWarrantyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateAssetWarrantyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private static UpdateAssetWarrantyCommand ValidUpdateCommand() =>
            new UpdateAssetWarrantyCommand
            {
                Id = 1,
                AssetId = 1,
                WarrantyType = 1,
                Description = "Updated Warranty",
                IsActive = 1
            };

        private void SetupHappyPath()
        {
            var dto = new AssetWarrantyDTO { Id = 1, AssetId = 1 };
            var entity = new AssetWarranties { Id = 1, AssetId = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetWarranties>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<AssetWarranties>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<AssetWarranties>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AssetWarrantyDTO)null!);

            var sut = CreateSut();
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidUpdateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsException()
        {
            var dto = new AssetWarrantyDTO { Id = 1, AssetId = 1 };
            var entity = new AssetWarranties { Id = 1, AssetId = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetWarranties>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<AssetWarranties>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(ValidUpdateCommand(), CancellationToken.None));
        }
    }
}
