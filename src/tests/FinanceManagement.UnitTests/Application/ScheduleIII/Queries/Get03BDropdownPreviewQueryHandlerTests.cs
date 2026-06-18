using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Queries
{
    public sealed class Get03BDropdownPreviewQueryHandlerTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        public Get03BDropdownPreviewQueryHandlerTests()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetDivisionId()).Returns(7);
        }

        private Get03BDropdownPreviewQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_ReturnsBsAndPlLeaves()
        {
            var preview = new Preview03BDto
            {
                BalanceSheetLeaves = new List<Preview03BItemDto> { new() { LineName = "Inventories" } },
                ProfitAndLossLeaves = new List<Preview03BItemDto>
                {
                    new() { LineName = "Cost of Materials Consumed" },
                    new() { LineName = "Other Income" }
                }
            };
            _mockQueryRepo.Setup(r => r.Get03BPreviewAsync(1, 7)).ReturnsAsync(preview);

            var result = await CreateSut().Handle(new Get03BDropdownPreviewQuery(), CancellationToken.None);

            result.BalanceSheetLeaves.Should().HaveCount(1);
            result.ProfitAndLossLeaves.Should().HaveCount(2);
        }
    }
}
