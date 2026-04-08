using PartyManagement.Application.GST.DTOs;
using PartyManagement.Application.GST.Queries;
using PartyManagement.Application.Interfaces.GST;

namespace PartyManagement.UnitTests.Application.GST.Queries
{
    public sealed class GetGSTINDetailsQueryHandlerTests
    {
        private readonly Mock<IGSTAuthService> _mockGstService = new(MockBehavior.Strict);

        private GetGSTINDetailsQueryHandler CreateSut() => new(_mockGstService.Object);

        [Fact]
        public async Task Handle_ValidGstin_ReturnsDetails()
        {
            var dto = new GSTINDetailsDto { Gstin = "22AAAAA1234A1Z5" };
            _mockGstService.Setup(s => s.GetGSTINDetailsAsync("22AAAAA1234A1Z5")).ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetGSTINDetailsQuery("22AAAAA1234A1Z5"), CancellationToken.None);
            result.Should().NotBeNull();
            result.Gstin.Should().Be("22AAAAA1234A1Z5");
        }
    }
}
