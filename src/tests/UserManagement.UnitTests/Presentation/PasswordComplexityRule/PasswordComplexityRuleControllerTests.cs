using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule;
using UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.PasswordComplexityRule
{
    public sealed class PasswordComplexityRuleControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<PasswordComplexityRuleController>> _mockLogger = new();

        private PasswordComplexityRuleController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAll_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetPwdRuleDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetPwdRuleDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetPasswordComplexityAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPwdRuleDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleAutoComplete>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PwdComplexityRuleAutoCompleteDto>());

            var result = await CreateSut().Getpwdautocomplete("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePasswordComplexityRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PwdRuleDto());

            var result = await CreateSut().CreateAsync(new CreatePasswordComplexityRuleCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPwdComplexityRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPwdRuleDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePasswordComplexityRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdatePasswordComplexityRuleCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
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
    }
}
