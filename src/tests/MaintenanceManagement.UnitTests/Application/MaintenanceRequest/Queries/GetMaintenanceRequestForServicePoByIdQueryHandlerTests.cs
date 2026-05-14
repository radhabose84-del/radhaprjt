using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Maintenance;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestForServicePoById;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries
{
    public sealed class GetMaintenanceRequestForServicePoByIdQueryHandlerTests
    {
        private readonly Mock<IMaintenanceRequestLookup> _mockLookup = new(MockBehavior.Strict);

        private GetMaintenanceRequestForServicePoByIdQueryHandler CreateSut() => new(_mockLookup.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess_When_Found()
        {
            var dto = new MaintenanceRequestLookupDto { Id = 501, RequestNo = "MR-501" };
            _mockLookup
                .Setup(l => l.GetByIdAsync(501, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMaintenanceRequestForServicePoByIdQuery { Id = 501 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Id.Should().Be(501);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_When_NotFound()
        {
            _mockLookup
                .Setup(l => l.GetByIdAsync(999999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenanceRequestLookupDto?)null);

            var result = await CreateSut().Handle(
                new GetMaintenanceRequestForServicePoByIdQuery { Id = 999999 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_DelegatesId_To_Lookup()
        {
            _mockLookup
                .Setup(l => l.GetByIdAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenanceRequestLookupDto?)null);

            await CreateSut().Handle(
                new GetMaintenanceRequestForServicePoByIdQuery { Id = 42 },
                CancellationToken.None);

            _mockLookup.Verify(l => l.GetByIdAsync(42, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
