using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<ProjectManagement.Domain.Entities.MiscTypeMaster>
            {
                MiscTypeMasterBuilders.ValidEntity()
            };
            var dtos = new List<GetMiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<ProjectManagement.Domain.Entities.MiscTypeMaster>
            {
                MiscTypeMasterBuilders.ValidEntity()
            };
            var dtos = new List<GetMiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(2, 5, "search"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyEntities = new List<ProjectManagement.Domain.Entities.MiscTypeMaster>();
            var emptyDtos = new List<GetMiscTypeMasterDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((emptyEntities, 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(emptyEntities))
                .Returns(emptyDtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<ProjectManagement.Domain.Entities.MiscTypeMaster>();
            var dtos = new List<GetMiscTypeMasterDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(entities))
                .Returns(dtos);

            await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
