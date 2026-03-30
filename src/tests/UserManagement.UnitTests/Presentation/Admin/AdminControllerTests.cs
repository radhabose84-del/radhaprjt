using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin;
using UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword;
using UserManagement.Application.EntityLevelAdmin.Commands.SendOTP;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.Admin
{
    public sealed class AdminControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private AdminController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEntityLevelAdminCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateEntityLevelAdminCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEntityLevelAdminCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateEntityLevelAdminCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateEntityLevelAdminCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOTP_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendOTPCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendOTPDTO());

            var result = await CreateSut().SendOTP(new SendOTPCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SetAdminPassword_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().SetAdminPassword(new ResetPasswordCommand());

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
