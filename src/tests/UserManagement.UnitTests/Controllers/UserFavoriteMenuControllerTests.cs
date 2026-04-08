using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Dto;
using UserManagement.Application.UserFavoriteMenu.Queries.GetUserFavoriteMenus;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class UserFavoriteMenuControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private UserFavoriteMenuController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetUserFavoriteMenusAsync_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserFavoriteMenusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserFavoriteMenuDto>());

            var result = await CreateSut().GetUserFavoriteMenusAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserFavoriteMenusAsync_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserFavoriteMenusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserFavoriteMenuDto>());

            await CreateSut().GetUserFavoriteMenusAsync();

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetUserFavoriteMenusQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddUserFavoriteMenu_ReturnsOkResult()
        {
            var command = new AddUserFavoriteMenuCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<AddUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Menu added to favorites.",
                    Data = 1
                });

            var result = await CreateSut().AddUserFavoriteMenu(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AddUserFavoriteMenu_CallsMediatorSendOnce()
        {
            var command = new AddUserFavoriteMenuCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<AddUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Menu added to favorites.",
                    Data = 1
                });

            await CreateSut().AddUserFavoriteMenu(command);

            _mockSender.Verify(
                m => m.Send(It.IsAny<AddUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveUserFavoriteMenu_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<RemoveUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().RemoveUserFavoriteMenu(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RemoveUserFavoriteMenu_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<RemoveUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().RemoveUserFavoriteMenu(1);

            _mockSender.Verify(
                m => m.Send(It.IsAny<RemoveUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
