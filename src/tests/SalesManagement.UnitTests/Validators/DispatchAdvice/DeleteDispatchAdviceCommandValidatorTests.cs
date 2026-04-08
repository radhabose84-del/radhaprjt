using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice;
using SalesManagement.Presentation.Validation.DispatchAdvice;

namespace SalesManagement.UnitTests.Validators.DispatchAdvice;

public sealed class DeleteDispatchAdviceCommandValidatorTests
{
    private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private DeleteDispatchAdviceCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.HasInvoiceAsync(id)).ReturnsAsync(false);
    }

    [Fact]
    public async Task ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(new DeleteDispatchAdviceCommand(1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(new DeleteDispatchAdviceCommand(0));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.HasInvoiceAsync(99)).ReturnsAsync(false);

        var result = await CreateValidator().TestValidateAsync(new DeleteDispatchAdviceCommand(99));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task HasInvoice_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.HasInvoiceAsync(1)).ReturnsAsync(true);

        var result = await CreateValidator().TestValidateAsync(new DeleteDispatchAdviceCommand(1));
        result.ShouldHaveAnyValidationError();
    }
}
