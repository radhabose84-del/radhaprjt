using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UserGroup.Commands.CreateUserGroup;
using UserManagement.Application.UserGroup.Commands.DeleteUserGroup;
using UserManagement.Application.UserGroup.Commands.UpdateUesrGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete;
using UserManagement.Application.UserGroup.Queries.GetUserGroupById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.UserGroup
{
    public sealed class UserGroupControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private UserGroupController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UserGroupDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<UserGroupDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCountriesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserGroupDto());

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
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUserGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserGroupDto());

            var result = await CreateSut().CreateAsync(new CreateUserGroupCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUserGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateUserGroupCommand { Id = 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().UpdateAsync(new UpdateUserGroupCommand { Id = 0 });

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUserGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserGroupDto());

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetUserGroup_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserGroupAutoCompleteDto>());

            var result = await CreateSut().GetUserGroup("Test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
