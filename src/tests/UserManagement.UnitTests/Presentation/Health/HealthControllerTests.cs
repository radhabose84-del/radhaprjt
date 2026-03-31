using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.Health
{
    public sealed class HealthControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private HealthController CreateSut() => new(_mockMediator.Object);

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

        [Fact]
        public void Get_DoesNotCallMediator()
        {
            CreateSut().Get();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<IRequest<object>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
