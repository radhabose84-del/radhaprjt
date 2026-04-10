using AutoMapper;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetCostTypeQuery;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;

namespace FixedAssetManagement.UnitTests.Application.AssetAdditionalCost.Queries
{
    public sealed class GetCostTypeQueryHandlerTests
    {
        private readonly Mock<IAssetAdditionalCostQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetCostTypeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var miscMasters = new List<FAM.Domain.Entities.MiscMaster>
            {
                new FAM.Domain.Entities.MiscMaster { Id = 1, Code = "CT001", Description = "Cost Type 1" }
            };

            _mockQueryRepo
                .Setup(r => r.GetCostTypeAsync())
                .ReturnsAsync(miscMasters);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>
                {
                    new GetMiscMasterDto { Id = 1, Code = "CT001", Description = "Cost Type 1" }
                });

            var result = await CreateSut().Handle(new GetCostTypeQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetCostTypeAsync())
                .ReturnsAsync(new List<FAM.Domain.Entities.MiscMaster>());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetCostTypeQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
