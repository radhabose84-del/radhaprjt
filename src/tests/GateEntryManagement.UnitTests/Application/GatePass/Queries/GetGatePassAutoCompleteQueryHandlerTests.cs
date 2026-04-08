using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassAutoComplete;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GatePass.Queries
{
    public sealed class GetGatePassAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IGatePassQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGatePassAutoCompleteQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var data = new List<GatePassAutoCompleteDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>())).ReturnsAsync(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetGatePassAutoCompleteQuery("test"), CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
