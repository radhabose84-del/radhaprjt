using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges;
using UserManagement.Application.RoleEntitlements.Commands.UpdateRoleRntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlementById;
using UserManagement.Application.RoleEntitlements.Queries.GetRolePrivileges;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class RoleEntitlementsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<RoleEntitlementsController>> _mockLogger = new();

        private RoleEntitlementsController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleEntitlementByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetByIdRoleEntitlementDTO());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- CreateRoleEntitlement ---

        [Fact]
        public async Task CreateRoleEntitlement_ReturnsOkResult()
        {
            var command = new CreateRoleEntitlementCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRoleEntitlementCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().CreateRoleEntitlement(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- UpdateRoleEntitlement ---

        [Fact]
        public async Task UpdateRoleEntitlement_ReturnsOkResult()
        {
            var command = new UpdateRoleEntitlementCommand { RoleId = 1 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateRoleEntitlementCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateRoleEntitlement(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetAllRolePrivilegesAsync ---

        [Fact]
        public async Task GetAllRolePrivilegesAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRolePrivilegesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ModuleDTO> { new ModuleDTO() });

            var result = await CreateSut().GetAllRolePrivilegesAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
