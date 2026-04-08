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

namespace UserManagement.UnitTests.Controllers
{
    public sealed class UserGroupControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private UserGroupController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAllCountriesAsync_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UserGroupDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<UserGroupDto> { new UserGroupDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCountriesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCountriesAsync_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UserGroupDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<UserGroupDto> { new UserGroupDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllCountriesAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetUserGroupQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserGroupDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateUserGroupCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateUserGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserGroupDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ValidId_ReturnsOkResult()
        {
            var command = new UpdateUserGroupCommand { Id = 1 };

            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateUserGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_InvalidId_ReturnsBadRequest()
        {
            var command = new UpdateUserGroupCommand { Id = 0 };

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
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
        public async Task GetUserGroup_AutoComplete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserGroupAutoCompleteDto>());

            var result = await CreateSut().GetUserGroup("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserGroup_AutoComplete_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserGroupAutoCompleteDto>());

            await CreateSut().GetUserGroup("test");

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetUserGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
