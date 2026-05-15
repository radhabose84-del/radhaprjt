using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Maintenance;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestsForServicePo;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries
{
    public sealed class GetMaintenanceRequestsForServicePoQueryHandlerTests
    {
        private readonly Mock<IMaintenanceRequestLookup> _mockLookup = new(MockBehavior.Strict);

        private GetMaintenanceRequestsForServicePoQueryHandler CreateSut() => new(_mockLookup.Object);

        [Fact]
        public async Task Handle_PassesSearchTerm_To_Lookup()
        {
            _mockLookup
                .Setup(l => l.GetAvailableForServicePoAsync("breakdown", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MaintenanceRequestLookupDto>());

            await CreateSut().Handle(
                new GetMaintenanceRequestsForServicePoQuery { SearchTerm = "breakdown" },
                CancellationToken.None);

            _mockLookup.Verify(
                l => l.GetAvailableForServicePoAsync("breakdown", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_With_Lookup_Results()
        {
            var rows = new List<MaintenanceRequestLookupDto>
            {
                new() { Id = 1, RequestNo = "MR-1" },
                new() { Id = 2, RequestNo = "MR-2" }
            };
            _mockLookup
                .Setup(l => l.GetAvailableForServicePoAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            var result = await CreateSut().Handle(
                new GetMaintenanceRequestsForServicePoQuery(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockLookup
                .Setup(l => l.GetAvailableForServicePoAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MaintenanceRequestLookupDto>());

            var result = await CreateSut().Handle(
                new GetMaintenanceRequestsForServicePoQuery(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
