using AutoMapper;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetBookTypeQuery;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationGroup.Queries
{
    public sealed class GetBookTypeQueryHandlerTests
    {
        private readonly Mock<IDepreciationGroupQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetBookTypeQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsBookTypeList()
        {
            var entities = new List<FAM.Domain.Entities.MiscMaster> { new() { Id = 1 } };
            var dtos = new List<GetMiscMasterDto> { new() { Id = 1 } };

            _mockRepo
                .Setup(r => r.GetBookTypeAsync())
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(new GetBookTypeQuery(), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetBookTypeAsync())
                .ReturnsAsync(new List<FAM.Domain.Entities.MiscMaster>());
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetBookTypeQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
