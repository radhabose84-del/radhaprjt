using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Queries
{
    public sealed class GetPoServiceHeaderByPoIdQueryHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPoServiceHeaderByPoIdQuery { PoId = 42 };
            query.PoId.Should().Be(42);
        }
    }
}
