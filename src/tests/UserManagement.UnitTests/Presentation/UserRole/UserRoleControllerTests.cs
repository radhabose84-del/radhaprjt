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

namespace UserManagement.UnitTests.Presentation.UserRole
{
    public sealed class UserRoleControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IUserCommandRepository> _mockUserCommandRepo = new();
        private readonly Mock<ILogger<UserRoleController>> _mockLogger = new();

        private UserRoleController CreateSut() =>
            new(_mockMediator.Object, _mockUserCommandRepo.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAll_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUserRoleDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetUserRoleDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllRoleAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUserRoleDto>>
                {
                    IsSuccess = false,
                    Data = new List<GetUserRoleDto>()
                });

            var result = await CreateSut().GetAllRoleAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetUserRoleDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetRoles_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRolesAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUserRoleAutocompleteDto>());

            var result = await CreateSut().GetRoles("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserRoleDto());

            var result = await CreateSut().CreateAsync(new CreateRoleCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await CreateSut().DeleteAsync(999);

            // DeleteRoleCommand returns int; the controller's null-check (CS0472) is always false,
            // so the Ok path is always reached regardless of the returned value.
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetUserRoleDto { Id = 1 });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateRoleCommand { Id = 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetUserRoleDto)null!);

            var result = await CreateSut().UpdateAsync(new UpdateRoleCommand { Id = 999 });

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
