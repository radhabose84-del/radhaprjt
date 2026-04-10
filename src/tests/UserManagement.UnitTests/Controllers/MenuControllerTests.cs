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

namespace UserManagement.UnitTests.Controllers
{
    public sealed class MenuControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private MenuController CreateSut() =>
            new(_mockMediator.Object);

        // --- GetParentMenuByModule ---

        [Fact]
        public async Task GetParentMenuByModule_ValidIds_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMenuByModuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MenuDTO> { new MenuDTO() });

            var result = await CreateSut().GetParentMenuByModule(new List<int> { 1, 2 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetParentMenuByModule_EmptyIds_ReturnsBadRequest()
        {
            var result = await CreateSut().GetParentMenuByModule(new List<int>());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- GetChildMenuByModule ---

        [Fact]
        public async Task GetChildMenuByModule_ValidIds_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetChildMenuByModuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ChildMenuDTO> { new ChildMenuDTO() });

            var result = await CreateSut().GetChildMenuByModule(new List<int> { 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetChildMenuByModule_EmptyIds_ReturnsBadRequest()
        {
            var result = await CreateSut().GetChildMenuByModule(new List<int>());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- GetAllMenusAsync ---

        [Fact]
        public async Task GetAllMenusAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMenuQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MenuDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<MenuDto> { new MenuDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMenusAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateMenuCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Update ---

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            var command = new UpdateMenuCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- UploadPreventiveSchedule (bulk upload) ---

        [Fact]
        public async Task UploadPreventiveSchedule_ReturnsOkResult()
        {
            var command = new UploadMenuCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadMenuCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Upload successful");

            var result = await CreateSut().UploadPreventiveSchedule(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetMenu (AutoComplete) ---

        [Fact]
        public async Task GetMenu_AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetParentMenuQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ParentMenuDto>());

            var result = await CreateSut().GetMenu("test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
