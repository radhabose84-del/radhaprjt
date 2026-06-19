using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetLinesAutoComplete;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Queries
{
    public sealed class GetLinesAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        public GetLinesAutoCompleteQueryHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetDivisionId()).Returns(7);
        }

        private GetLinesAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ResolvesTokenAndReturnsLines()
        {
            var data = new List<ScheduleIIILineLookupDto>
            {
                new() { DetailId = 1, DisplayOrder = 1, LineCode = "PPE", LineName = "Property, Plant and Equipment" }
            };
            _mockQueryRepo.Setup(r => r.GetLinesAutoCompleteAsync(1, 7, "ppe")).ReturnsAsync(data);

            var result = await CreateSut().Handle(new GetLinesAutoCompleteQuery { Term = "ppe" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }
    }
}
