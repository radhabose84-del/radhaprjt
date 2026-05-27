using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class DeleteReturnTypeValidatorTests
{
    private readonly Mock<IReturnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private DeleteReturnTypeValidator CreateValidator() => new(_mockQueryRepo.Object);

    [Fact]
    public async Task Validate_ValidId_NotFoundFalse_NotLinked_Passes()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(false);

        var command = ReturnTypeBuilders.ValidDeleteCommand(1);
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ZeroId_FailsValidation()
    {
        var command = ReturnTypeBuilders.ValidDeleteCommand(0);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        var command = ReturnTypeBuilders.ValidDeleteCommand(99);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_LinkedToOtherRecords_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

        var command = ReturnTypeBuilders.ValidDeleteCommand(1);
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
