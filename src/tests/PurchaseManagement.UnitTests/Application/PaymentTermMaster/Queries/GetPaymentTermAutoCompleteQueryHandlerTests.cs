using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete;

namespace PurchaseManagement.UnitTests.Application.PaymentTermMaster.Queries
{
    public sealed class GetPaymentTermAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IPaymentTermMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetPaymentTermAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingItems()
        {
            var items = new List<AutoCompleteDto>
            {
                new AutoCompleteDto { Id = 1, Code = "PT001", Description = "30 Days" }
            };
            _mockQueryRepo
                .Setup(r => r.GetPaymentTermAutoComplete("30", null))
                .ReturnsAsync(items);

            var result = await CreateSut().Handle(
                new GetPaymentTermAutoCompleteQuery { SearchPattern = "30", PaymentTermCode = null },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("PT001");
        }

        [Fact]
        public async Task Handle_NullResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetPaymentTermAutoComplete(null, null))
                .ReturnsAsync((List<AutoCompleteDto>)null!);

            var result = await CreateSut().Handle(
                new GetPaymentTermAutoCompleteQuery { SearchPattern = null, PaymentTermCode = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ByExactCode_ReturnsItems()
        {
            var items = new List<AutoCompleteDto>
            {
                new AutoCompleteDto { Id = 1, Code = "PT001", Description = "30 Days" }
            };
            _mockQueryRepo
                .Setup(r => r.GetPaymentTermAutoComplete(null, "PT001"))
                .ReturnsAsync(items);

            var result = await CreateSut().Handle(
                new GetPaymentTermAutoCompleteQuery { SearchPattern = null, PaymentTermCode = "PT001" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
