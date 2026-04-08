using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class PasswordComplexityRuleControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<PasswordComplexityRuleController>> _mockLogger = new();

        private PasswordComplexityRuleController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        // --- GetPasswordComplexityAsync ---

        [Fact]
        public async Task GetPasswordComplexityAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetPwdRuleDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetPwdRuleDto> { new GetPwdRuleDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetPasswordComplexityAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPasswordComplexityAsync_NullResult_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApiResponseDTO<List<GetPwdRuleDto>>?)null);

            var result = await CreateSut().GetPasswordComplexityAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPwdRuleDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Getpwdautocomplete ---

        [Fact]
        public async Task Getpwdautocomplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleAutoComplete>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PwdComplexityRuleAutoCompleteDto>());

            var result = await CreateSut().Getpwdautocomplete("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreatePasswordComplexityRuleCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePasswordComplexityRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PwdRuleDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_ExistingRule_ReturnsOkResult()
        {
            var command = new UpdatePasswordComplexityRuleCommand { Id = 1 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPwdRuleDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePasswordComplexityRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ReturnsBadRequest()
        {
            var command = new UpdatePasswordComplexityRuleCommand { Id = 99 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetPwdRuleDto?)null);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ExistingRule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPwdRuleDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePasswordComplexityRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetPwdRuleDto?)null);

            var result = await CreateSut().Delete(99);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
