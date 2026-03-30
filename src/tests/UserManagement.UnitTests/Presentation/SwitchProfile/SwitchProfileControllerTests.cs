using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.SwitchProfile.Commands.SwitchProfileByUnit;
using UserManagement.Application.SwitchProfile.Queries.GetUnitProfile;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.SwitchProfile
{
    public sealed class SwitchProfileControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private SwitchProfileController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetUnit_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUnitProfileDTO>());

            var result = await CreateSut().GetUnit();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnit_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUnitProfileDTO>());

            await CreateSut().GetUnit();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetUnitProfileQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SwitchProfile_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SwitchProfileByUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SwitchProfileByUnitDTO());

            var result = await CreateSut().SwitchProfile(new SwitchProfileByUnitCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SwitchProfile_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SwitchProfileByUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SwitchProfileByUnitDTO());

            await CreateSut().SwitchProfile(new SwitchProfileByUnitCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<SwitchProfileByUnitCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
