using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocationById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.UserRoleAllocation
{
    public sealed class UserRoleAllocationControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private UserRoleAllocationController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateRoleAllocation_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUserRoleAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserRoleAllocationResponseDto>());

            var result = await CreateSut().CreateRoleAllocation(new CreateUserRoleAllocationDto());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteRoleAllocation_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRoleAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await CreateSut().DeleteRoleAllocation(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteRoleAllocation_Success_ReturnsNoContent()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRoleAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteRoleAllocation(1);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task GetUserRoleAllocations_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserRoleAllocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CreateUserRoleAllocationDto>());

            var result = await CreateSut().GetUserRoleAllocations();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserRoleAllocationById_Found_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserRoleAllocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateUserRoleAllocationDto());

            var result = await CreateSut().GetUserRoleAllocationById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserRoleAllocationById_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserRoleAllocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateUserRoleAllocationDto)null!);

            var result = await CreateSut().GetUserRoleAllocationById(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
