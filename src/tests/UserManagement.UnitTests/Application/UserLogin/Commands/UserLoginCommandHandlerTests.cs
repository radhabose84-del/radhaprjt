using Contracts.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Application.UserLogin.Commands.UserLogin;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Application.UserLogin.Commands
{
    public sealed class UserLoginCommandHandlerTests
    {
        private readonly Mock<IJwtTokenHelper> _mockJwt = new(MockBehavior.Loose);
        private readonly Mock<IUserQueryRepository> _mockUserQuery = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UserLoginCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IUserSessionRepository> _mockSession = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttpContext = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);
        private readonly Mock<ILoginPolicyFactory> _mockLoginPolicy = new(MockBehavior.Loose);

        private UserLoginCommandHandler CreateSut()
        {
            _mockJwtSettings
                .Setup(o => o.Value)
                .Returns(new JwtSettings { SecretKey = "TestKey12345678901234567890123456", Issuer = "test", Audience = "test" });

            return new UserLoginCommandHandler(
                _mockJwt.Object, _mockUserQuery.Object, _mockMediator.Object,
                _mockLogger.Object, _mockSession.Object, _mockHttpContext.Object,
                _mockIpService.Object, _mockJwtSettings.Object, _mockTimeZone.Object,
                _mockLoginPolicy.Object);
        }

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NullUsername_ReturnsFailure()
        {
            var sut = CreateSut();

            var result = await sut.Handle(
                new UserLoginCommand { Username = null!, Password = "pass" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
