using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Queries
{
    public sealed class GetAssetTypeQueryHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private FAM.Application.DepreciationGroup.Queries.GetAssetTypeQuery.GetAssetTypeQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.MiscMaster> { new() { Id = 1 } };
            var dtos = new List<GetMiscMasterDto> { new() { Id = 1 } };

            _mockRepo
                .Setup(r => r.GetAssetTypeAsync())
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new FAM.Application.DepreciationGroup.Queries.GetAssetTypeQuery.GetAssetTypeQuery(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockRepo
                .Setup(r => r.GetAssetTypeAsync())
                .ReturnsAsync(new List<FAM.Domain.Entities.MiscMaster>());
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(
                new FAM.Application.DepreciationGroup.Queries.GetAssetTypeQuery.GetAssetTypeQuery(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
