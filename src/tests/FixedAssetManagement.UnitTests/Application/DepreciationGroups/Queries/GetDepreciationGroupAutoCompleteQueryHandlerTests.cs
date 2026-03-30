using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupAutoComplete;
using FAM.Domain.Common;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationGroups.Queries
{
    public sealed class GetDepreciationGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDepreciationGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepreciationGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidPattern_ReturnsDtoList()
        {
            var dtoList = new List<DepreciationGroupDTO> { DepreciationGroupBuilders.ValidDto() };
            var autoCompleteDtoList = new List<DepreciationGroupAutoCompleteDTO> { DepreciationGroupBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByDepreciationNameAsync(It.IsAny<string>()))
                .ReturnsAsync(dtoList);

            _mockMapper
                .Setup(m => m.Map<List<DepreciationGroupAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(autoCompleteDtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepreciationGroupAutoCompleteQuery { SearchPattern = "DG" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullPattern_ThrowsEntityNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByDepreciationNameAsync(It.IsAny<string>()))
                .ReturnsAsync((List<DepreciationGroupDTO>)null!);

            var sut = CreateSut();

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                sut.Handle(new GetDepreciationGroupAutoCompleteQuery { SearchPattern = null }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidPattern_PublishesAuditEvent()
        {
            var dtoList = new List<DepreciationGroupDTO> { DepreciationGroupBuilders.ValidDto() };
            var autoCompleteDtoList = new List<DepreciationGroupAutoCompleteDTO> { DepreciationGroupBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByDepreciationNameAsync(It.IsAny<string>()))
                .ReturnsAsync(dtoList);

            _mockMapper
                .Setup(m => m.Map<List<DepreciationGroupAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(autoCompleteDtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetDepreciationGroupAutoCompleteQuery { SearchPattern = "DG" },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
