using AutoMapper;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsuranceById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Domain.Events;
using FluentValidation;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetInsurance.Queries
{
    public sealed class GetAssetInsuranceByIdQueryHandlerTests
    {
        private readonly Mock<IAssetInsuranceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetInsuranceByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = AssetInsuranceBuilders.ValidEntity(1);
            var dto = AssetInsuranceBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByAssetIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetAssetInsuranceDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetInsuranceByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsValidationException()
        {
            var dto = AssetInsuranceBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByAssetIdAsync(It.IsAny<int>()))
                .ReturnsAsync((FAM.Domain.Entities.AssetMaster.AssetInsurance?)null);

            _mockMapper
                .Setup(m => m.Map<GetAssetInsuranceDto>(It.IsAny<object?>()))
                .Returns(dto);

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(new GetAssetInsuranceByIdQuery { Id = 99 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = AssetInsuranceBuilders.ValidEntity(1);
            var dto = AssetInsuranceBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByAssetIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetAssetInsuranceDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetInsuranceByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
