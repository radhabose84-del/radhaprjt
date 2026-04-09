using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Modules.Commands.CreateModule;
using UserManagement.Application.Modules.Commands.DeleteModule;
using UserManagement.Application.Modules.Commands.UpdateModule;
using UserManagement.Application.Modules.Queries.GetModuleAutoComplete;
using UserManagement.Application.Modules.Queries.GetModuleById;
using UserManagement.Application.Modules.Queries.GetModules;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class ModulesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<ModulesController>> _mockLogger = new();

        private ModulesController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        // --- CreateModule ---

        [Fact]
        public async Task CreateModule_ReturnsOkResult()
        {
            var command = new CreateModuleCommand { ModuleName = "TestModule" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateModuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateModule(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetAllModuleAsync ---

        [Fact]
        public async Task GetAllModuleAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetModulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ModuleDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<ModuleDto> { new ModuleDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllModuleAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Found_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetModuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModuleByIdDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetModuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ModuleByIdDto?)null);

            var result = await CreateSut().GetByIdAsync(99);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- UpdateModule ---

        [Fact]
        public async Task UpdateModule_ReturnsOkResult()
        {
            var command = new UpdateModuleCommand { ModuleName = "Updated" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateModuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateModule(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- DeleteModule ---

        [Fact]
        public async Task DeleteModule_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteModuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteModule(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteModule_InvalidId_ReturnsBadRequest()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteModuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteModule(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- GetModule (AutoComplete) ---

        [Fact]
        public async Task GetModule_AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetModuleAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ModuleAutoCompleteDTO>());

            var result = await CreateSut().GetModule("test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
