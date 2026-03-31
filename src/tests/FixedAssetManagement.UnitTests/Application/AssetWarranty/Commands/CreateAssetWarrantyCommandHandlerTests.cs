using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Commands
{
    public sealed class CreateAssetWarrantyCommandHandlerTests
    {
        private readonly Mock<IAssetWarrantyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateAssetWarrantyCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            var entity = AssetWarrantyBuilders.ValidEntity(1);
            var dto = AssetWarrantyBuilders.ValidDto(1);

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetIdAsync(It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<AssetWarranties>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<AssetWarranties>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AssetWarrantyDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();
            var command = AssetWarrantyBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = AssetWarrantyBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<AssetWarranties>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = AssetWarrantyBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WarrantyAlreadyExists_ThrowsValidationException()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByAssetIdAsync(It.IsAny<int?>()))
                .ReturnsAsync(true);

            var command = AssetWarrantyBuilders.ValidCreateCommand();

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ThrowsException()
        {
            var entity = AssetWarrantyBuilders.ValidEntity(0); // Id = 0
            var dto = AssetWarrantyBuilders.ValidDto(0);       // Id = 0

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetIdAsync(It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<AssetWarranties>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<AssetWarranties>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AssetWarrantyDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(AssetWarrantyBuilders.ValidCreateCommand(), CancellationToken.None));
        }
    }
}
