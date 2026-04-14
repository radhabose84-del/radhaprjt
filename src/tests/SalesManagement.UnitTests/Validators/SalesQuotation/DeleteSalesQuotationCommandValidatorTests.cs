using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.DeleteSalesQuotation;
using SalesManagement.Presentation.Validation.SalesQuotation;

namespace SalesManagement.UnitTests.Validators.SalesQuotation;

public sealed class DeleteSalesQuotationCommandValidatorTests
{
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

    private DeleteSalesQuotationCommandValidator CreateValidator() => new(_mockQueryRepo.Object, _mockAccessFilter.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
    }

    [Fact]
    public async Task ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(new DeleteSalesQuotationCommand(1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(new DeleteSalesQuotationCommand(0));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        var result = await CreateValidator().TestValidateAsync(new DeleteSalesQuotationCommand(99));
        result.ShouldHaveAnyValidationError();
    }
}
