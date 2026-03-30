using AutoMapper;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationGroups.Queries
{
    public sealed class GetDepreciationGroupQueryHandlerTests
    {
        private readonly Mock<IDepreciationGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepreciationGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsListAndCount()
        {
            var dtoList = new List<DepreciationGroupDTO> { DepreciationGroupBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllDepreciationGroupAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            _mockMapper
                .Setup(m => m.Map<List<DepreciationGroupDTO>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var (result, count) = await CreateSut().Handle(
                new GetDepreciationGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().HaveCount(1);
            count.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var emptyList = new List<DepreciationGroupDTO>();
            _mockQueryRepo
                .Setup(r => r.GetAllDepreciationGroupAsync(1, 10, null))
                .ReturnsAsync((emptyList, 0));

            _mockMapper
                .Setup(m => m.Map<List<DepreciationGroupDTO>>(It.IsAny<object>()))
                .Returns(emptyList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var (result, count) = await CreateSut().Handle(
                new GetDepreciationGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().BeEmpty();
            count.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithSearchTerm_PassesTermToRepository()
        {
            var dtoList = new List<DepreciationGroupDTO>();
            _mockQueryRepo
                .Setup(r => r.GetAllDepreciationGroupAsync(1, 10, "test"))
                .ReturnsAsync((dtoList, 0));

            _mockMapper
                .Setup(m => m.Map<List<DepreciationGroupDTO>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetDepreciationGroupQuery { PageNumber = 1, PageSize = 10, SearchTerm = "test" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllDepreciationGroupAsync(1, 10, "test"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<DepreciationGroupDTO>();
            _mockQueryRepo
                .Setup(r => r.GetAllDepreciationGroupAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((dtoList, 0));

            _mockMapper
                .Setup(m => m.Map<List<DepreciationGroupDTO>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetDepreciationGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
