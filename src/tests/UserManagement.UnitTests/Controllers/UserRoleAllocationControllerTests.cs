using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocationById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class UserRoleAllocationControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private UserRoleAllocationController CreateSut() => new(_mockSender.Object);

        // --- CreateRoleAllocation ---

        [Fact]
        public async Task CreateRoleAllocation_ReturnsOkResult()
        {
            var dto = new CreateUserRoleAllocationDto { UserId = 1, RoleIds = new List<int> { 1, 2 } };

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateUserRoleAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserRoleAllocationResponseDto>());

            var result = await CreateSut().CreateRoleAllocation(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateRoleAllocation_CallsMediatorSendOnce()
        {
            var dto = new CreateUserRoleAllocationDto { UserId = 1, RoleIds = new List<int> { 1 } };

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateUserRoleAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserRoleAllocationResponseDto>());

            await CreateSut().CreateRoleAllocation(dto);

            _mockSender.Verify(
                m => m.Send(It.IsAny<CreateUserRoleAllocationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- DeleteRoleAllocation ---

        [Fact]
        public async Task DeleteRoleAllocation_ExistingId_ReturnsNoContent()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteRoleAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteRoleAllocation(1);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteRoleAllocation_NonExistingId_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteRoleAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await CreateSut().DeleteRoleAllocation(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetUserRoleAllocations ---

        [Fact]
        public async Task GetUserRoleAllocations_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserRoleAllocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CreateUserRoleAllocationDto>());

            var result = await CreateSut().GetUserRoleAllocations();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserRoleAllocations_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserRoleAllocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CreateUserRoleAllocationDto>());

            await CreateSut().GetUserRoleAllocations();

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetUserRoleAllocationQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- GetUserRoleAllocationById ---

        [Fact]
        public async Task GetUserRoleAllocationById_ExistingUserId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserRoleAllocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateUserRoleAllocationDto { UserId = 1, RoleIds = new List<int> { 1 } });

            var result = await CreateSut().GetUserRoleAllocationById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserRoleAllocationById_NonExistingUserId_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserRoleAllocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateUserRoleAllocationDto?)null!);

            var result = await CreateSut().GetUserRoleAllocationById(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
