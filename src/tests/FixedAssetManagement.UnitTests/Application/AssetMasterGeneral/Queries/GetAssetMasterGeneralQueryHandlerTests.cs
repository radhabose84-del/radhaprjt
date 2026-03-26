using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Queries
{
    public sealed class GetAssetMasterGeneralQueryHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetMasterGeneralQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<AssetMasterGeneralDTO> { AssetMasterGeneralBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetAsync(1, 10, null))
                .ReturnsAsync((dtos, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetMasterGeneralDTO>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetAssetMasterGeneralQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtos = new List<AssetMasterGeneralDTO> { AssetMasterGeneralBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetAsync(2, 5, "test"))
                .ReturnsAsync((dtos, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetMasterGeneralDTO>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetAssetMasterGeneralQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetAsync(1, 10, null))
                .ReturnsAsync((new List<AssetMasterGeneralDTO>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetMasterGeneralDTO>>(It.IsAny<object>()))
                .Returns(new List<AssetMasterGeneralDTO>());

            var result = await CreateSut().Handle(
                new GetAssetMasterGeneralQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
