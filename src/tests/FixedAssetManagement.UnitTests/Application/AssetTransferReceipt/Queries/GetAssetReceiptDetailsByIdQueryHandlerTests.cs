using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferReceipt.Queries
{
    public sealed class GetAssetReceiptDetailsByIdQueryHandlerTests
    {
        private readonly Mock<IAssetTransferReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetReceiptDetailsByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDtoList()
        {
            var entityList = new List<AssetReceiptDetailsByIdDto>
            {
                new AssetReceiptDetailsByIdDto { AssetReceiptId = 1, AssetCode = "AST001" }
            };

            _mockQueryRepo
                .Setup(r => r.GetByAssetReceiptId(1))
                .ReturnsAsync(entityList);

            _mockMapper
                .Setup(m => m.Map<List<AssetReceiptDetailsByIdDto>>(It.IsAny<object>()))
                .Returns(entityList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetReceiptDetailsByIdQuery { AssetReceiptId = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByAssetReceiptId(99))
                .ReturnsAsync((List<AssetReceiptDetailsByIdDto>?)null);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(
                    new GetAssetReceiptDetailsByIdQuery { AssetReceiptId = 99 },
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyList_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByAssetReceiptId(99))
                .ReturnsAsync(new List<AssetReceiptDetailsByIdDto>());

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(
                    new GetAssetReceiptDetailsByIdQuery { AssetReceiptId = 99 },
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entityList = new List<AssetReceiptDetailsByIdDto>
            {
                new AssetReceiptDetailsByIdDto { AssetReceiptId = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetByAssetReceiptId(1))
                .ReturnsAsync(entityList);

            _mockMapper
                .Setup(m => m.Map<List<AssetReceiptDetailsByIdDto>>(It.IsAny<object>()))
                .Returns(entityList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetReceiptDetailsByIdQuery { AssetReceiptId = 1 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
