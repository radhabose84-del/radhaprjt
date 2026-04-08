using Contracts.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Application.SwitchProfile.Commands.SwitchProfileByUnit;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Application.SwitchProfile.Commands
{
    public sealed class SwitchProfileByUnitCommandHandlerTests
    {
        private readonly Mock<IJwtTokenHelper> _mockJwt = new(MockBehavior.Loose);
        private readonly Mock<IUserSessionRepository> _mockSessionRepo = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttpCtx = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUserQueryRepository> _mockUserRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        [Fact]
        public async Task Handle_UserNotFound_ThrowsValidationException()
        {
            _mockIpService.Setup(i => i.GetUserId()).Returns(1);
            _mockIpService.Setup(i => i.GetGroupCode()).Returns("ADMIN");
            _mockUserRepo.Setup(r => r.GetByUserByUnit(1, 5)).ReturnsAsync((UserManagement.Domain.Entities.User?)null);

            var jwtSettings = Options.Create(new JwtSettings());
            var sut = new SwitchProfileByUnitCommandHandler(_mockJwt.Object, _mockSessionRepo.Object, _mockHttpCtx.Object,
                _mockTimeZone.Object, _mockMediator.Object, _mockUserRepo.Object, _mockIpService.Object, jwtSettings);

            Func<Task> act = async () => await sut.Handle(
                new SwitchProfileByUnitCommand { UnitId = 5, CompanyId = 1, DivisionId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
