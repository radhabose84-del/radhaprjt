using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad;
using PartyManagement.Domain.Events;

namespace PartyManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetPartyGroupLoadQueryHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPartyGroupLoadQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsGroups()
        {
            var data = new List<PartyGroupLoadDto> { new() };
            _mockRepo.Setup(r => r.GetPartyGroupsAsync(It.IsAny<List<int>>())).ReturnsAsync(data);
            _mockMapper.Setup(m => m.Map<List<PartyGroupLoadDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetPartyGroupLoadQuery { GroupTypeIds = new List<int> { 1 } }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
