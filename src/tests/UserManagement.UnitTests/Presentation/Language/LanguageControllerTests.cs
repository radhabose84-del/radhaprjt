using Contracts.Common;
using UserManagement.Application.Language.Commands.CreateLanguage;
using UserManagement.Application.Language.Commands.UpdateLanguage;
using UserManagement.Application.Language.Commands.DeleteLanguage;
using UserManagement.Application.Language.Queries.GetLanguages;
using UserManagement.Application.Language.Queries.GetLanguageById;
using UserManagement.Application.Language.Queries.GetLanguageAutoComplete;
using UserManagement.Presentation.Controllers;
using UserManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.UnitTests.Presentation.Language
{
    public sealed class LanguageControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private LanguageController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllLanguages_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LanguageDTO>>
                {
                    IsSuccess = true,
                    Data = new List<LanguageDTO> { LanguageBuilders.ValidDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllLanguagesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllLanguages_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LanguageDTO>>
                {
                    IsSuccess = true,
                    Data = new List<LanguageDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllLanguagesAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetLanguageQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LanguageBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            var command = LanguageBuilders.ValidCreateCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateLanguageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LanguageBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_LanguageExists_ReturnsOkResult()
        {
            var command = LanguageBuilders.ValidUpdateCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LanguageBuilders.ValidDto());
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateLanguageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_LanguageNotFound_ReturnsNotFound()
        {
            var command = LanguageBuilders.ValidUpdateCommand(id: 999);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LanguageDTO?)null!);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteLanguageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLanguageAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LanguageAutoCompleteDTO> { LanguageBuilders.ValidAutoCompleteDto() });

            var result = await CreateSut().GetLanguage("Eng");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
