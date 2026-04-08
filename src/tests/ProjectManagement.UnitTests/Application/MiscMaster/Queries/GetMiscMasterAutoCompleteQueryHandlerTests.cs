using AutoMapper;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using ProjectManagement.Domain.Events;

namespace ProjectManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var entities = new List<ProjectManagement.Domain.Entities.MiscMaster> { new() { Id = 1 } };
            var data = new List<GetMiscMasterAutoCompleteDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetMiscMaster("test", "TYPE")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(entities)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscMasterAutoCompleteQuery { SearchPattern = "test", MiscTypeCode = "TYPE" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
