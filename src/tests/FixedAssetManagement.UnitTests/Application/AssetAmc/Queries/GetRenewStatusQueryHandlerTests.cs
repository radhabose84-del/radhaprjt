using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetRenewStatus;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAmc.Queries
{
    public sealed class GetRenewStatusQueryHandlerTests
    {
        private readonly Mock<IAssetAmcQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetRenewStatusQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.MiscMaster> { new() { Id = 1 } };
            var dtos = new List<GetMiscMasterDto> { new() { Id = 1 } };

            _mockRepo
                .Setup(r => r.GetRenewStatusAsync())
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(new GetRenewStatusQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockRepo
                .Setup(r => r.GetRenewStatusAsync())
                .ReturnsAsync(new List<FAM.Domain.Entities.MiscMaster>());
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetRenewStatusQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
