using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Commands
{
    public sealed class DeleteAssetMasterGeneralCommandHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAssetMasterGeneralCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidDelete_ReturnsDto()
        {
            var dto = AssetMasterGeneralBuilders.ValidDto();
            var entity = AssetMasterGeneralBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetMasterGenerals>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<AssetMasterGenerals>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<AssetMasterGeneralDTO>(It.IsAny<object>()))
                .Returns(new AssetMasterGeneralDTO { Id = 1 });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(AssetMasterGeneralBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((AssetMasterGeneralDTO?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                CreateSut().Handle(new DeleteAssetMasterGeneralCommand { Id = 99 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            var dto = AssetMasterGeneralBuilders.ValidDto();
            var entity = AssetMasterGeneralBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetMasterGenerals>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<AssetMasterGenerals>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<ExceptionRules>(() =>
                CreateSut().Handle(AssetMasterGeneralBuilders.ValidDeleteCommand(), CancellationToken.None));
        }
    }
}
