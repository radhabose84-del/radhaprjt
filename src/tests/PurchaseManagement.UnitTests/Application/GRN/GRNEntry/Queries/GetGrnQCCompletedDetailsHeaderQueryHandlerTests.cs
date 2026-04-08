using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Queries
{
    public sealed class GetGrnQCCompletedDetailsHeaderQueryHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);

        private GetGrnQCCompletedDetailsHeaderQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockPartyLookup.Object, _mockWarehouseLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetGrnQCCompletedDetailsHeaderQuery
            {
                FromDate = DateTimeOffset.UtcNow.AddDays(-7),
                ToDate = DateTimeOffset.UtcNow,
                PageNumber = 1,
                PageSize = 15,
                SearchTerm = "search"
            };
            query.PageNumber.Should().Be(1);
            query.SearchTerm.Should().Be("search");
        }
    }
}
