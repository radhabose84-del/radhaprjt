using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSpecification.Queries
{
    public sealed class GetAssetSpecificationQueryHandlerTests
    {
        private readonly Mock<IAssetSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSpecificationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var jsonDtos = new List<AssetSpecificationJsonDto> { AssetSpecificationBuilders.ValidJsonDto(1) };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetSpecificationAsync(1, 10, null))
                .ReturnsAsync((jsonDtos, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetSpecificationJsonDto>>(It.IsAny<object>()))
                .Returns(jsonDtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSpecificationQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var jsonDtos = new List<AssetSpecificationJsonDto> { AssetSpecificationBuilders.ValidJsonDto(1) };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetSpecificationAsync(2, 5, "test"))
                .ReturnsAsync((jsonDtos, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetSpecificationJsonDto>>(It.IsAny<object>()))
                .Returns(jsonDtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSpecificationQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetSpecificationAsync(1, 10, null))
                .ReturnsAsync((new List<AssetSpecificationJsonDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetSpecificationJsonDto>>(It.IsAny<object>()))
                .Returns(new List<AssetSpecificationJsonDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSpecificationQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
