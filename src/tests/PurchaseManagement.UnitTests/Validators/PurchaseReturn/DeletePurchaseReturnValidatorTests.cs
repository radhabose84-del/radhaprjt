using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class DeletePurchaseReturnValidatorTests
{
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private DeletePurchaseReturnValidator CreateValidator() => new(_mockQueryRepo.Object);

    [Fact]
    public async Task Validate_Valid_Passes()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
        var result = await CreateValidator().TestValidateAsync(PurchaseReturnBuilders.ValidDeleteCommand(1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ZeroId_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(PurchaseReturnBuilders.ValidDeleteCommand(0));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        var result = await CreateValidator().TestValidateAsync(PurchaseReturnBuilders.ValidDeleteCommand(99));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
