using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Commands.DeleteSalesReturn;
using SalesManagement.Presentation.Validation.SalesReturn;

namespace SalesManagement.UnitTests.Validators.SalesReturn;

public sealed class DeleteSalesReturnCommandValidatorTests
{
    private readonly Mock<ISalesReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private DeleteSalesReturnCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
    }

    [Fact]
    public async Task ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(new DeleteSalesReturnCommand(1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(new DeleteSalesReturnCommand(0));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        var result = await CreateValidator().TestValidateAsync(new DeleteSalesReturnCommand(99));
        result.ShouldHaveAnyValidationError();
    }
}
