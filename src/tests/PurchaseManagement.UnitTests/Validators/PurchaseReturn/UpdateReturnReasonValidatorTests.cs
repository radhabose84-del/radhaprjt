using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class UpdateReturnReasonValidatorTests
{
    private readonly Mock<IReturnReasonQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private UpdateReturnReasonValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupHappyPath()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.ReturnTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupHappyPath();
        var command = ReturnReasonBuilders.ValidUpdateCommand();
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        var command = ReturnReasonBuilders.ValidUpdateCommand(id: 99);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_EmptyDescription_FailsValidation()
    {
        SetupHappyPath();
        var command = ReturnReasonBuilders.ValidUpdateCommand(description: "");
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_ZeroReturnTypeId_FailsValidation()
    {
        SetupHappyPath();
        var command = ReturnReasonBuilders.ValidUpdateCommand(returnTypeId: 0);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ReturnTypeId);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-1)]
    public async Task Validate_IsActiveOutOfRange_FailsValidation(int isActive)
    {
        SetupHappyPath();
        var command = ReturnReasonBuilders.ValidUpdateCommand(isActive: isActive);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.IsActive);
    }
}
