using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Commands
{
    public sealed class DeleteAssetWarrantyCommandHandlerTests
    {
        private readonly Mock<IAssetWarrantyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetWarrantyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAssetWarrantyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = AssetWarrantyBuilders.ValidDto(1);
            var entity = AssetWarrantyBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetWarranties>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<AssetWarranties>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<AssetWarrantyDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                AssetWarrantyBuilders.ValidDeleteCommand(1), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((AssetWarrantyDTO)null!);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(AssetWarrantyBuilders.ValidDeleteCommand(99), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            var dto = AssetWarrantyBuilders.ValidDto(1);
            var entity = AssetWarrantyBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetWarranties>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<AssetWarranties>()))
                .ReturnsAsync(0);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(AssetWarrantyBuilders.ValidDeleteCommand(1), CancellationToken.None));
        }
    }
}
