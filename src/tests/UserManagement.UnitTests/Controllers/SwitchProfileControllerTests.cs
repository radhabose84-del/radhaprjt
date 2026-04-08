using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.SwitchProfile.Commands.SwitchProfileByUnit;
using UserManagement.Application.SwitchProfile.Queries.GetUnitProfile;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class SwitchProfileControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private SwitchProfileController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetUnit_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUnitProfileDTO>());

            var result = await CreateSut().GetUnit();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnit_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUnitProfileDTO>());

            await CreateSut().GetUnit();

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetUnitProfileQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SwitchProfile_ReturnsOkResult()
        {
            var command = new SwitchProfileByUnitCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<SwitchProfileByUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SwitchProfileByUnitDTO());

            var result = await CreateSut().SwitchProfile(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SwitchProfile_CallsMediatorSendOnce()
        {
            var command = new SwitchProfileByUnitCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<SwitchProfileByUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SwitchProfileByUnitDTO());

            await CreateSut().SwitchProfile(command);

            _mockSender.Verify(
                m => m.Send(It.IsAny<SwitchProfileByUnitCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
