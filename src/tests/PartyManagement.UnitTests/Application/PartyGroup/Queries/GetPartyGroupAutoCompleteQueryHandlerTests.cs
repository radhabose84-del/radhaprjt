using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using PartyManagement.Domain.Events;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyGroup.Queries
{
    public sealed class GetPartyGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IPartyGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPartyGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsAutoCompleteList()
        {
            var repoResult = new List<PartyGroupAutoCompleteDto>
            {
                new PartyGroupAutoCompleteDto { Id = 1, PartyGroupName = "Group A" }
            };

            _mockQueryRepo
                .Setup(r => r.GetMainPartyGroups(It.IsAny<string>()))
                .ReturnsAsync(repoResult);

            _mockMapper
                .Setup(m => m.Map<List<PartyGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(repoResult);

            var result = await CreateSut().Handle(
                new GetPartyGroupAutoCompleteQuery { SearchPattern = "Group" }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmpty()
        {
            var emptyList = new List<PartyGroupAutoCompleteDto>();

            _mockQueryRepo
                .Setup(r => r.GetMainPartyGroups(It.IsAny<string>()))
                .ReturnsAsync(emptyList);

            _mockMapper
                .Setup(m => m.Map<List<PartyGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(emptyList);

            var result = await CreateSut().Handle(
                new GetPartyGroupAutoCompleteQuery { SearchPattern = "NoMatch" }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var repoResult = new List<PartyGroupAutoCompleteDto>();

            _mockQueryRepo
                .Setup(r => r.GetMainPartyGroups(It.IsAny<string>()))
                .ReturnsAsync(repoResult);

            _mockMapper
                .Setup(m => m.Map<List<PartyGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(repoResult);

            await CreateSut().Handle(
                new GetPartyGroupAutoCompleteQuery { SearchPattern = "Test" }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
