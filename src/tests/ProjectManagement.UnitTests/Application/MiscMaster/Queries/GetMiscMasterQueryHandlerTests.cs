using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMaster;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterQueryHanlder CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<ProjectManagement.Domain.Entities.MiscMaster>
            {
                MiscMasterBuilders.ValidEntity()
            };
            var dtos = new List<GetMiscMasterDto> { MiscMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyEntities = new List<ProjectManagement.Domain.Entities.MiscMaster>();
            var emptyDtos = new List<GetMiscMasterDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(1, 15, null))
                .ReturnsAsync((emptyEntities, 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(emptyEntities))
                .Returns(emptyDtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<ProjectManagement.Domain.Entities.MiscMaster>
            {
                MiscMasterBuilders.ValidEntity()
            };
            var dtos = new List<GetMiscMasterDto> { MiscMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(2, 5, "test"))
                .ReturnsAsync((entities, 20));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }
    }
}
