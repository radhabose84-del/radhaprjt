using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Gate;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetGrnPendingHeaderQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);
        private readonly Mock<IGateInwardLookup> _mockGateInwardLookup = new(MockBehavior.Loose);

        private GetGrnPendingHeaderQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockPartyLookup.Object, _mockWarehouseLookup.Object, _mockGateInwardLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetGrnPendingHeaderQuery
            {
                FromDate = DateTimeOffset.UtcNow.AddDays(-7),
                ToDate = DateTimeOffset.UtcNow,
                IsGrnGenerated = true,
                IsQcGenerated = false,
                PageNumber = 2,
                PageSize = 20,
                SearchTerm = "test"
            };
            query.PageNumber.Should().Be(2);
            query.PageSize.Should().Be(20);
            query.SearchTerm.Should().Be("test");
        }
    }
}
