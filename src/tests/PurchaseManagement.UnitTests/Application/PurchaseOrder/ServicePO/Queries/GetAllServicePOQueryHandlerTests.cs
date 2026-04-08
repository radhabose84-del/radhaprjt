using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePO;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Queries
{
    public sealed class GetAllServicePOQueryHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private GetServicePOByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockCurrencyLookup.Object, _mockUomLookup.Object);

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetServicePOByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseOrderServiceDetailDto?)null);

            var result = await CreateSut().Handle(new GetServicePOByIdQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetServicePOByIdQuery(42);
            query.Id.Should().Be(42);
        }
    }
}
