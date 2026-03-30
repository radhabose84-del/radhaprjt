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

namespace UserManagement.UnitTests.Presentation.Modules
{
    public sealed class ModulesControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<ModulesController>> _mockLogger = new();

        private ModulesController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task CreateModule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateModuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateModule(new CreateModuleCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetModulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ModuleDto>>
                {
                    IsSuccess = true,
                    Data = new List<ModuleDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllModuleAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_Found_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetModuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModuleByIdDto { Id = 1 });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateModule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateModuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateModule(new UpdateModuleCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

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
        public async Task GetModule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetModuleAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ModuleAutoCompleteDTO>());

            var result = await CreateSut().GetModule("Test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
