using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Menu.Commands.CreateMenu;
using UserManagement.Application.Menu.Commands.DeleteMenu;
using UserManagement.Application.Menu.Commands.UpdateMenu;
using UserManagement.Application.Menu.Commands.UploadMenu;
using UserManagement.Application.Menu.Queries.GetChildMenuByModule;
using UserManagement.Application.Menu.Queries.GetMenu;
using UserManagement.Application.Menu.Queries.GetMenuByModule;
using UserManagement.Application.Menu.Queries.GetParentMenu;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.Menu
{
    public sealed class MenuControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private MenuController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetParentMenuByModule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMenuByModuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MenuDTO>());

            var result = await CreateSut().GetParentMenuByModule(new List<int> { 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetChildMenuByModule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetChildMenuByModuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ChildMenuDTO>());

            var result = await CreateSut().GetChildMenuByModule(new List<int> { 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMenuQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MenuDto>>
                {
                    IsSuccess = true,
                    Data = new List<MenuDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMenusAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateMenuCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateMenuCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadPreventiveSchedule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("success");

            var result = await CreateSut().UploadPreventiveSchedule(new UploadMenuCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMenu_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetParentMenuQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ParentMenuDto>());

            var result = await CreateSut().GetMenu("Test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
