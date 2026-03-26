using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSpecification.Queries
{
    public sealed class GetAssetSpecificationByIdQueryHandlerTests
    {
        private readonly Mock<IAssetSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSpecificationByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var jsonDto = AssetSpecificationBuilders.ValidJsonDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(jsonDto);

            _mockMapper
                .Setup(m => m.Map<AssetSpecificationJsonDto>(It.IsAny<object>()))
                .Returns(jsonDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSpecificationByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.AssetId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((AssetSpecificationJsonDto)null!);

            _mockMapper
                .Setup(m => m.Map<AssetSpecificationJsonDto>(It.IsAny<object>()))
                .Returns((AssetSpecificationJsonDto)null!);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new GetAssetSpecificationByIdQuery { Id = 99 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var jsonDto = AssetSpecificationBuilders.ValidJsonDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(jsonDto);

            _mockMapper
                .Setup(m => m.Map<AssetSpecificationJsonDto>(It.IsAny<object>()))
                .Returns(jsonDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetSpecificationByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
