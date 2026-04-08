using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Commands.DeleteStoHeader;
using SalesManagement.Presentation.Validation.StoHeader;

namespace SalesManagement.UnitTests.Validators.StoHeader;

public sealed class DeleteStoHeaderCommandValidatorTests
{
    private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private DeleteStoHeaderCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
    }

    [Fact]
    public async Task ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(new DeleteStoHeaderCommand(1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(new DeleteStoHeaderCommand(0));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        var result = await CreateValidator().TestValidateAsync(new DeleteStoHeaderCommand(99));
        result.ShouldHaveAnyValidationError();
    }
}
