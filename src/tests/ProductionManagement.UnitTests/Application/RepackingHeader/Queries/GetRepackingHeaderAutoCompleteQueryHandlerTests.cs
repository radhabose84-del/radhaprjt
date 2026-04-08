using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderAutoComplete;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.UnitTests.Application.RepackingHeader.Queries
{
    public sealed class GetRepackingHeaderAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRepackingHeaderAutoCompleteQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var data = new List<RepackingHeaderLookupDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>(), null)).ReturnsAsync(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetRepackingHeaderAutoCompleteQuery("test", null), CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
