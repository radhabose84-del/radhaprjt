using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class CreateReturnReasonValidatorTests
{
    private readonly Mock<IReturnReasonQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private CreateReturnReasonValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupHappyPath()
    {
        _mockQueryRepo.Setup(r => r.ReturnTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null)).ReturnsAsync(false);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupHappyPath();
        var command = ReturnReasonBuilders.ValidCreateCommand();
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_EmptyCode_FailsValidation(string? code)
    {
        var command = ReturnReasonBuilders.ValidCreateCommand(code: code!);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_NonAlphanumericCode_FailsValidation()
    {
        var command = ReturnReasonBuilders.ValidCreateCommand(code: "x-y");
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_ZeroReturnTypeId_FailsValidation()
    {
        var command = ReturnReasonBuilders.ValidCreateCommand(returnTypeId: 0);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ReturnTypeId);
    }

    [Fact]
    public async Task Validate_ReturnTypeNotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.ReturnTypeExistsAsync(999)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null)).ReturnsAsync(false);

        var command = ReturnReasonBuilders.ValidCreateCommand(returnTypeId: 999);
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.ReturnTypeId);
    }

    [Fact]
    public async Task Validate_DuplicateCodeForType_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.ReturnTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("MoistureFailure", 1, null)).ReturnsAsync(true);

        var command = ReturnReasonBuilders.ValidCreateCommand();
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveAnyValidationError();
    }
}
