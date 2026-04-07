using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails;
using PurchaseManagement.Presentation.Controllers;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class PartyMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private PartyMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetPartyDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPartyDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyMasterDTO>());

            var result = await CreateSut().GetPartyDetails("UNIT01", "search");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPartyDetails_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPartyDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyMasterDTO>());

            await CreateSut().GetPartyDetails("UNIT01", "search");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetPartyDetailsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPartyDetails_WithNullParams_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPartyDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyMasterDTO>());

            var result = await CreateSut().GetPartyDetails(null!, null!);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
