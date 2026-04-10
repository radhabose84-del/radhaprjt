using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetChildPartyGroupAutoComplete;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using PartyManagement.Domain.Events;

namespace PartyManagement.UnitTests.Application.PartyGroup.Queries
{
    public sealed class GetChildPartyGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IPartyGroupQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetChildPartyGroupAutoCompleteQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var data = new List<PartyGroupAutoCompleteDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetParentPartyGroups("test")).ReturnsAsync(data);
            _mockMapper.Setup(m => m.Map<List<PartyGroupAutoCompleteDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetChildPartyGroupAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
