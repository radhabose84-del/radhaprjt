using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartyManagement.Application.GST.DTOs;
using PartyManagement.Application.GST.Queries;
using PartyManagement.Application.Interfaces.GST;
using WebAPI.Controllers;

namespace PartyManagement.UnitTests.Controllers
{
    public sealed class GSTControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IGSTAuthService> _mockGstService = new(MockBehavior.Strict);

        private GSTController CreateSut() => new(_mockMediator.Object, _mockGstService.Object);

        [Fact]
        public async Task GetAuthToken_ReturnsOkResult()
        {
            _mockGstService
                .Setup(s => s.GetAuthTokenAsync())
                .ReturnsAsync(new GSTAuthResponseDto());

            var result = await CreateSut().GetAuthToken();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAuthToken_CallsGstService_Once()
        {
            _mockGstService
                .Setup(s => s.GetAuthTokenAsync())
                .ReturnsAsync(new GSTAuthResponseDto());

            await CreateSut().GetAuthToken();

            _mockGstService.Verify(s => s.GetAuthTokenAsync(), Times.Once);
        }

        [Fact]
        public async Task GetGSTDetails_ReturnsOkResult_WhenFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGSTINDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GSTINDetailsDto());

            var result = await CreateSut().GetGSTDetails("22AAAAA1234A1Z5");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetGSTDetails_ReturnsNotFound_WhenNull()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGSTINDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GSTINDetailsDto)null!);

            var result = await CreateSut().GetGSTDetails("00AAAAA0000A0Z0");

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetGSTDetails_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGSTINDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GSTINDetailsDto());

            await CreateSut().GetGSTDetails("22AAAAA1234A1Z5");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetGSTINDetailsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
