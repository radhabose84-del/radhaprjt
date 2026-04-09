using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES;

namespace PurchaseManagement.UnitTests.Application.ServiceMaster.Queries
{
    public sealed class GetServiceEntrySheetsWithActivitiesByPoIdHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private GetServiceEntrySheetsWithActivitiesByPoIdHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            _mockRepo
                .Setup(r => r.GetByPurchaseOrderIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<ServiceEntrySheetWithActivitiesDto>());

            var result = await CreateSut().Handle(
                new GetServiceEntrySheetsWithActivitiesByPoIdQuery(99), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetServiceEntrySheetsWithActivitiesByPoIdQuery(42);
            query.PurchaseOrderId.Should().Be(42);
        }
    }
}
