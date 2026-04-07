using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPartyMaster;
using PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetPartyDetailsQueryHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPartyDetailsQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsPartyList()
        {
            var rawList = new List<PartyMasterDTO>
            {
                new PartyMasterDTO { Code = "V001", Slname = "Vendor One" }
            };
            _mockRepo
                .Setup(r => r.GetPartyMasters("UNIT01", "search"))
                .ReturnsAsync(rawList);
            _mockMapper
                .Setup(m => m.Map<List<PartyMasterDTO>>(rawList))
                .Returns(rawList);

            var result = await CreateSut().Handle(
                new GetPartyDetailsQuery { OldunitCode = "UNIT01", SearchPattern = "search" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("V001");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var emptyList = new List<PartyMasterDTO>();
            _mockRepo
                .Setup(r => r.GetPartyMasters("UNIT01", "none"))
                .ReturnsAsync(emptyList);
            _mockMapper
                .Setup(m => m.Map<List<PartyMasterDTO>>(emptyList))
                .Returns(emptyList);

            var result = await CreateSut().Handle(
                new GetPartyDetailsQuery { OldunitCode = "UNIT01", SearchPattern = "none" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var rawList = new List<PartyMasterDTO>();
            _mockRepo
                .Setup(r => r.GetPartyMasters(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(rawList);
            _mockMapper
                .Setup(m => m.Map<List<PartyMasterDTO>>(rawList))
                .Returns(rawList);

            await CreateSut().Handle(
                new GetPartyDetailsQuery { OldunitCode = "U", SearchPattern = "s" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
