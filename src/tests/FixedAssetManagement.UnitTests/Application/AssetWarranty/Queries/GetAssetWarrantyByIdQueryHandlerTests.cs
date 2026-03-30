using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Queries
{
    public sealed class GetAssetWarrantyByIdQueryHandlerTests
    {
        private readonly Mock<IAssetWarrantyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetWarrantyByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = AssetWarrantyBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetWarrantyDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetWarrantyByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((AssetWarrantyDTO)null!);

            _mockMapper
                .Setup(m => m.Map<AssetWarrantyDTO>(It.IsAny<object>()))
                .Returns((AssetWarrantyDTO)null!);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new GetAssetWarrantyByIdQuery { Id = 99 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var dto = AssetWarrantyBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<AssetWarrantyDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetWarrantyByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
