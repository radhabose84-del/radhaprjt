using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSpecification.Commands
{
    public sealed class DeleteAssetSpecificationCommandHandlerTests
    {
        private readonly Mock<IAssetSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAssetSpecificationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var jsonDto = AssetSpecificationBuilders.ValidJsonDto(1);
            var entity = AssetSpecificationBuilders.ValidEntity(1);
            var specDto = AssetSpecificationBuilders.ValidSpecificationDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(jsonDto);

            _mockMapper
                .Setup(m => m.Map<AssetSpecifications>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<AssetSpecifications>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<AssetSpecificationDTO>(It.IsAny<object>()))
                .Returns(specDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                AssetSpecificationBuilders.ValidDeleteCommand(1), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((AssetSpecificationJsonDto)null!);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(AssetSpecificationBuilders.ValidDeleteCommand(99), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            var jsonDto = AssetSpecificationBuilders.ValidJsonDto(1);
            var entity = AssetSpecificationBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(jsonDto);

            _mockMapper
                .Setup(m => m.Map<AssetSpecifications>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<AssetSpecifications>()))
                .ReturnsAsync(0);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(AssetSpecificationBuilders.ValidDeleteCommand(1), CancellationToken.None));
        }
    }
}
