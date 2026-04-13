using AutoMapper;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using MaintenanceManagement.Application.MachineMaster.Queries.GetAssetSpecificationById;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Queries.Batch2
{
    public sealed class GetAssetSpecificationByIdQueryHandlerTests
    {
        private readonly Mock<IAssetSpecificationLookup> _mockLookup = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSpecificationByIdQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockLookup.Object);

        [Fact]
        public async Task Handle_WithSpecifications_ReturnsSuccess()
        {
            _mockLookup
                .Setup(l => l.GetByAssetIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetSpecificationLookupDto>
                {
                    new() { AssetId = 1, SpecificationName = "Make", SpecificationValue = "BSOFT" }
                });

            var result = await CreateSut().Handle(
                new GetAssetSpecificationByIdQuery { AssetId = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Message.Should().Be("Specifications found.");
        }

        [Fact]
        public async Task Handle_WithNoSpecifications_ReturnsSuccessWithEmptyMessage()
        {
            _mockLookup
                .Setup(l => l.GetByAssetIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetSpecificationLookupDto>());

            var result = await CreateSut().Handle(
                new GetAssetSpecificationByIdQuery { AssetId = 2 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.Message.Should().Be("No specifications found for the asset.");
        }

        [Fact]
        public async Task Handle_TotalCountMatchesData()
        {
            _mockLookup
                .Setup(l => l.GetByAssetIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetSpecificationLookupDto>
                {
                    new() { AssetId = 1 },
                    new() { AssetId = 1 }
                });

            var result = await CreateSut().Handle(
                new GetAssetSpecificationByIdQuery { AssetId = 1 },
                CancellationToken.None);

            result.TotalCount.Should().Be(2);
        }
    }
}
