using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.BillEntry.Queries
{
    public sealed class GetAllPurchaseBillEntryQueryHandlerTests
    {
        private readonly Mock<IPurchaseBillEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetPurchaseBillEntryListQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyVm()
        {
            _mockRepo
                .Setup(r => r.GetListAsync(
                    It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<DateOnly?>(),
                    It.IsAny<DateOnly?>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<PurchaseBillEntryHeader>)new List<PurchaseBillEntryHeader>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<PurchaseBillEntryHeaderDto>>(It.IsAny<object>()))
                .Returns(new List<PurchaseBillEntryHeaderDto>());

            var result = await CreateSut().Handle(
                new GetAllPurchaseBillEntryQuery { Page = 1, Size = 15 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.Total.Should().Be(0);
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetAllPurchaseBillEntryQuery
            {
                VendorId = 1,
                Search = "test",
                FromDate = new DateOnly(2026, 1, 1),
                ToDate = new DateOnly(2026, 12, 31),
                Page = 2,
                Size = 10
            };
            query.VendorId.Should().Be(1);
            query.Page.Should().Be(2);
        }
    }
}
