using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.UserRole.Commands.CreateRole;
using UserManagement.Application.UserRole.Commands.DeleteRole;
using UserManagement.Application.UserRole.Commands.UpdateRole;
using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Application.UserRole.Queries.GetRoleById;
using UserManagement.Application.UserRole.Queries.GetRolesAutocomplete;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class UserRoleControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);
        private readonly Mock<IUserCommandRepository> _mockUserCommandRepo = new();
        private readonly Mock<ILogger<UserRoleController>> _mockLogger = new();

        private UserRoleController CreateSut() =>
            new(_mockSender.Object, _mockUserCommandRepo.Object, _mockLogger.Object);

        // --- GetAllRoleAsync ---

        [Fact]
        public async Task GetAllRoleAsync_WithData_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUserRoleDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetUserRoleDto> { new GetUserRoleDto() },
                    TotalCount = 1
                });

            var result = await CreateSut().GetAllRoleAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllRoleAsync_EmptyData_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUserRoleDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetUserRoleDto>(),
                    TotalCount = 0
                });

            var result = await CreateSut().GetAllRoleAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAllRoleAsync_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUserRoleDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetUserRoleDto> { new GetUserRoleDto() },
                    TotalCount = 1
                });

            await CreateSut().GetAllRoleAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetRoleQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetUserRoleDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetRoles (AutoComplete) ---

        [Fact]
        public async Task GetRoles_AutoComplete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRolesAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUserRoleAutocompleteDto>());

            var result = await CreateSut().GetRoles("admin");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetRoles_AutoComplete_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRolesAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUserRoleAutocompleteDto>());

            await CreateSut().GetRoles("admin");

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetRolesAutocompleteQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateRoleCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserRoleDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- DeleteAsync ---

        [Fact]
        public async Task DeleteAsync_ExistingRole_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.Is<DeleteRoleCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // The controller sends the delete command twice (first to check, then to delete)
            // Both calls need to be set up
            _mockSender
                .Setup(m => m.Send(It.Is<DeleteRoleCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_ExistingRole_ReturnsOkResult()
        {
            var command = new UpdateRoleCommand { Id = 1 };

            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetUserRoleDto());

            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_NonExistingRole_ReturnsNotFound()
        {
            var command = new UpdateRoleCommand { Id = 999 };

            _mockSender
                .Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetUserRoleDto?)null!);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
