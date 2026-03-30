using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.UpdateRoleRntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlementById;
using UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges;
using UserManagement.Application.RoleEntitlements.Queries.GetRolePrivileges;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.RoleEntitlements
{
    public sealed class RoleEntitlementsControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<RoleEntitlementsController>> _mockLogger = new();

        private RoleEntitlementsController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleEntitlementByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetByIdRoleEntitlementDTO());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateRoleEntitlement_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRoleEntitlementCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().CreateRoleEntitlement(new CreateRoleEntitlementCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateRoleEntitlement_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateRoleEntitlementCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateRoleEntitlement(new UpdateRoleEntitlementCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllRolePrivilegesAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRolePrivilegesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ModuleDTO>());

            var result = await CreateSut().GetAllRolePrivilegesAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
