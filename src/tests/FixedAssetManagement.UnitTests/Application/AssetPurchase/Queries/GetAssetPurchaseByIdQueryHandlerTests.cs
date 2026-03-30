using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchaseById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Queries
{
    public sealed class GetAssetPurchaseByIdQueryHandlerTests
    {
        private readonly Mock<IAssetPurchaseQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetPurchaseByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = AssetPurchaseBuilders.ValidEntity();
            var dto = AssetPurchaseBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AssetPurchaseDetailsDto>(It.IsAny<object>()))
                .Returns(dto);

            var result = await CreateSut().Handle(new GetAssetPurchaseByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails?)null);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new GetAssetPurchaseByIdQuery { Id = 99 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = AssetPurchaseBuilders.ValidEntity();
            var dto = AssetPurchaseBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AssetPurchaseDetailsDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetAssetPurchaseByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
