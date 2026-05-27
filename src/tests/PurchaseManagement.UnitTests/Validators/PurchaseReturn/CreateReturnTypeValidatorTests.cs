using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class CreateReturnTypeValidatorTests
{
    private readonly Mock<IReturnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private CreateReturnTypeValidator CreateValidator() => new(_mockQueryRepo.Object);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.InventoryImpactExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.FinanceImpactExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = ReturnTypeBuilders.ValidCreateCommand();
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_EmptyCode_FailsValidation(string? code)
    {
        var command = ReturnTypeBuilders.ValidCreateCommand(code: code!);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_NonAlphanumericCode_FailsValidation()
    {
        var command = ReturnTypeBuilders.ValidCreateCommand(code: "Code With Space");
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_DuplicateCode_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Rejected", null)).ReturnsAsync(true);

        var command = ReturnTypeBuilders.ValidCreateCommand(code: "Rejected");
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_EmptyDescription_FailsValidation()
    {
        var command = ReturnTypeBuilders.ValidCreateCommand(description: "");
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_InvalidInventoryImpactId_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.InventoryImpactExistsAsync(999)).ReturnsAsync(false);

        var command = ReturnTypeBuilders.ValidCreateCommand(inventoryImpactId: 999);
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor("InventoryImpactId.Value");
    }
}
