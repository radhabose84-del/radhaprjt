using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using ProjectManagement.Domain.Events;

namespace ProjectManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var entities = new List<ProjectManagement.Domain.Entities.MiscTypeMaster> { new() { Id = 1 } };
            var data = new List<GetMiscTypeMasterAutocompleteDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetMiscTypeMaster("test")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
