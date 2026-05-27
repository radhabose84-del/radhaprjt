using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetAllPurchaseOrder;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Queries
{
    public sealed class GetAllPurchaseOrderQueryHandlerTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IBudgetGroupLookup> _mockBudgetGroupLookup = new(MockBehavior.Loose);
        private readonly Mock<IInventoryCategoryLookup> _mockInventoryCategoryLookup = new(MockBehavior.Loose);

        private GetPurchaseOrdersQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockPartyLookup.Object, _mockBudgetGroupLookup.Object,
                _mockInventoryCategoryLookup.Object);

        private static PagedResult<PurchaseOrderListItemDto> EmptyPage() =>
            new() { Page = 1, PageSize = 20, Total = 0, Items = new List<PurchaseOrderListItemDto>() };

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPurchaseOrdersQuery(1, 15, "search", null, null, null);
            query.PageNumber.Should().Be(1);
            query.PageSize.Should().Be(15);
            query.SearchTerm.Should().Be("search");
        }

        [Fact]
        public async Task Handle_WhenNoPartyId_CallsGetAllAsync()
        {
            _mockIp.Setup(i => i.GetPartyId()).Returns((int?)null);
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyPage());

            await CreateSut().Handle(
                new GetPurchaseOrdersQuery(1, 20, null, null, null, null),
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockRepo.Verify(
                r => r.GetMyPurchaseOrdersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPartyIdPresent_CallsGetMyPurchaseOrdersAsync()
        {
            _mockIp.Setup(i => i.GetPartyId()).Returns(42);
            _mockRepo
                .Setup(r => r.GetMyPurchaseOrdersAsync(42, 1, 20, It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyPage());

            await CreateSut().Handle(
                new GetPurchaseOrdersQuery(1, 20, null, null, null, null),
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetMyPurchaseOrdersAsync(42, 1, 20, It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockRepo.Verify(
                r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPartyIdZero_CallsGetAllAsync()
        {
            _mockIp.Setup(i => i.GetPartyId()).Returns(0);
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyPage());

            await CreateSut().Handle(
                new GetPurchaseOrdersQuery(1, 20, null, null, null, null),
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockRepo.Verify(
                r => r.GetMyPurchaseOrdersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
