using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSpecification.Queries
{
    public sealed class GetAssetSpecificationBasedMachineNoQueryHandlerTests
    {
        private readonly Mock<IAssetSpecificationQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSpecificationBasedMachineNoQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<AssetSpecBasedOnMachineNoDto> { new() };
            var dtos = new List<AssetSpecBasedOnMachineNoDto> { new() };

            _mockRepo
                .Setup(r => r.GetAssetSpecBasedOnMachineNos(1, 15, null))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<AssetSpecBasedOnMachineNoDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSpecificationBasedMachineNoQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo
                .Setup(r => r.GetAssetSpecBasedOnMachineNos(1, 15, null))
                .ReturnsAsync((new List<AssetSpecBasedOnMachineNoDto>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<AssetSpecBasedOnMachineNoDto>>(It.IsAny<object>()))
                .Returns(new List<AssetSpecBasedOnMachineNoDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSpecificationBasedMachineNoQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
