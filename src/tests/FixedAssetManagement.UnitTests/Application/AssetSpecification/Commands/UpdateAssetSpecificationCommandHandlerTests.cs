using FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSpecification.Commands
{
    public sealed class UpdateAssetSpecificationCommandHandlerTests
    {
        private readonly Mock<IAssetSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateAssetSpecificationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private static UpdateAssetSpecificationCommand ValidUpdateCommand() =>
            new UpdateAssetSpecificationCommand
            {
                AssetId = 1,
                Specifications = new List<UpdateSpecificationItem>
                {
                    new UpdateSpecificationItem
                    {
                        SpecificationId = 10,
                        SpecificationValue = "150kg",
                        IsActive = 1
                    }
                }
            };

        [Fact]
        public async Task Handle_AssetNotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AssetSpecificationJsonDto)null!);

            var sut = CreateSut();
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidUpdateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SpecificationExists_CallsUpdate()
        {
            var dto = new AssetSpecificationJsonDto { AssetId = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetSpecIdAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<AssetSpecifications>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);

            result.Should().Be("Specifications updated successfully.");
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<AssetSpecifications>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SpecificationExists_PublishesAuditEvent()
        {
            var dto = new AssetSpecificationJsonDto { AssetId = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetSpecIdAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<AssetSpecifications>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SpecificationDoesNotExist_ReturnsNoUpdatesMessage()
        {
            var dto = new AssetSpecificationJsonDto { AssetId = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetSpecIdAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);

            result.Should().Be("No specifications updated.");
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<AssetSpecifications>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_SpecificationDoesNotExist_DoesNotPublishAuditEvent()
        {
            var dto = new AssetSpecificationJsonDto { AssetId = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetSpecIdAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
