using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Language.Commands.CreateLanguage;
using UserManagement.Application.Language.Commands.DeleteLanguage;
using UserManagement.Application.Language.Commands.UpdateLanguage;
using UserManagement.Application.Language.Queries.GetLanguageAutoComplete;
using UserManagement.Application.Language.Queries.GetLanguageById;
using UserManagement.Application.Language.Queries.GetLanguages;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class LanguageControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private LanguageController CreateSut() =>
            new(_mockMediator.Object);

        // --- GetAllLanguagesAsync ---

        [Fact]
        public async Task GetAllLanguagesAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LanguageDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<LanguageDTO> { new LanguageDTO() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllLanguagesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateLanguageCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateLanguageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguageDTO());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguageDTO());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Update ---

        [Fact]
        public async Task Update_ExistingLanguage_ReturnsOkResult()
        {
            var command = new UpdateLanguageCommand { Id = 1 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguageDTO());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateLanguageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            var command = new UpdateLanguageCommand { Id = 99 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LanguageDTO?)null);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteLanguageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetLanguage (AutoComplete) ---

        [Fact]
        public async Task GetLanguage_AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LanguageAutoCompleteDTO>());

            var result = await CreateSut().GetLanguage("en");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
