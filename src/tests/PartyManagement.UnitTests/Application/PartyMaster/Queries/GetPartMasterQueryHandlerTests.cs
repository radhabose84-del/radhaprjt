using AutoMapper;
using Contracts.Common;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMaster;
using PartyManagement.Domain.Events;

namespace PartyManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetPartMasterQueryHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPartMasterQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var data = new List<GetPartyMasterDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetAllPartyMasterAsync(1, 10, null)).ReturnsAsync((data, 1));
            _mockMapper.Setup(m => m.Map<List<GetPartyMasterDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetPartMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
