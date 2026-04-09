using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.EntityLevelAdmin.Commands.SendOTP;

namespace UserManagement.UnitTests.Application.EntityLevelAdmin.Commands
{
    public sealed class SendOTPCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IChangePassword> _mockChangePassword = new(MockBehavior.Strict);
        private readonly Mock<INotificationsQueryRepository> _mockNotifRepo = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);

        private SendOTPCommandHandler CreateSut() =>
            new(_mockMediator.Object, _mockMapper.Object, _mockChangePassword.Object, _mockNotifRepo.Object, _mockTimeZone.Object);

        [Fact]
        public void Constructor_AllDependenciesInjected_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
