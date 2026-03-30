using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Dto;
using UserManagement.Application.UserFavoriteMenu.Queries.GetUserFavoriteMenus;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.UserFavoriteMenu
{
    public sealed class UserFavoriteMenuControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private UserFavoriteMenuController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetUserFavoriteMenus_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserFavoriteMenusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserFavoriteMenuDto>());

            var result = await CreateSut().GetUserFavoriteMenusAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AddUserFavoriteMenu_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AddUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().AddUserFavoriteMenu(new AddUserFavoriteMenuCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RemoveUserFavoriteMenu_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<RemoveUserFavoriteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().RemoveUserFavoriteMenu(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
