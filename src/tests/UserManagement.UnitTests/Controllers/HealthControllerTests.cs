using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class HealthControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private HealthController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public void Get_ReturnsOkResult()
        {
            var result = CreateSut().Get();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Get_ReturnsHealthyMessage()
        {
            var result = CreateSut().Get() as OkObjectResult;

            result!.Value.Should().Be("Healthy");
        }
    }
}
