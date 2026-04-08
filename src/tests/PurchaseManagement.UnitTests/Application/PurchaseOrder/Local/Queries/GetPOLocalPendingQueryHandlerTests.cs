using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Queries
{
    /// <summary>
    /// The GetPOLocalPendingQueryHandler source is entirely commented out.
    /// These tests verify the query class properties only.
    /// </summary>
    public sealed class GetPOLocalPendingQueryHandlerTests
    {
        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPOLocalPendingQuery
            {
                PageNumber = 2,
                PageSize = 20,
                SearchTerm = "search",
                PoId = 5,
                PoMethodId = 3
            };
            query.PageNumber.Should().Be(2);
            query.PageSize.Should().Be(20);
            query.SearchTerm.Should().Be("search");
            query.PoId.Should().Be(5);
            query.PoMethodId.Should().Be(3);
        }

        [Fact]
        public void QueryClass_DefaultValues_ShouldBeSet()
        {
            var query = new GetPOLocalPendingQuery();
            query.PageNumber.Should().Be(1);
            query.PageSize.Should().Be(15);
            query.SearchTerm.Should().BeNull();
        }
    }
}
