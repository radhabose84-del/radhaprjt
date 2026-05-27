using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class UpdateReturnTypeValidatorTests
{
    private readonly Mock<IReturnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private UpdateReturnTypeValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupHappyPath()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.InventoryImpactExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.FinanceImpactExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupHappyPath();
        var command = ReturnTypeBuilders.ValidUpdateCommand();
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
        var command = ReturnTypeBuilders.ValidUpdateCommand(id: 999);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_ZeroId_FailsValidation()
    {
        var command = ReturnTypeBuilders.ValidUpdateCommand(id: 0);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_EmptyDescription_FailsValidation()
    {
        SetupHappyPath();
        var command = ReturnTypeBuilders.ValidUpdateCommand(description: "");
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-1)]
    public async Task Validate_IsActiveOutOfRange_FailsValidation(int isActive)
    {
        SetupHappyPath();
        var command = ReturnTypeBuilders.ValidUpdateCommand(isActive: isActive);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.IsActive);
    }
}
