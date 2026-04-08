using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterPending;
using PartyManagement.Domain.Events;

namespace PartyManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetPartyMasterPendingQueryHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetPartyMasterPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockUnitLookup.Object, _mockWorkflowLookup.Object, _mockUserLookup.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_ReturnsData()
        {
            _mockIpService.Setup(i => i.GetUserId()).Returns(1);
            var data = new List<PartyMasterPendingDto>();
            _mockRepo.Setup(r => r.GetAllPartyMasterPendingAsync(It.IsAny<string?>()))
                .ReturnsAsync((data, 0));
            _mockMapper.Setup(m => m.Map<List<PartyMasterPendingDto>>(It.IsAny<object>())).Returns(new List<PartyMasterPendingDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());
            _mockWorkflowLookup.Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<List<int>>()))
                .ReturnsAsync(new List<ApproverListDto>());
            _mockUserLookup.Setup(u => u.GetAllUserAsync()).ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UserLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetPartyMasterPendingQuery { SearchTerm = null }, CancellationToken.None);
            result.Data.Should().BeEmpty();
        }
    }
}
