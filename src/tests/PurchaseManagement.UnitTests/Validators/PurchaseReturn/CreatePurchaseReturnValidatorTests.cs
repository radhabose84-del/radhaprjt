using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class CreatePurchaseReturnValidatorTests
{
    private readonly Mock<IReturnReasonQueryRepository> _mockReasonRepo = new(MockBehavior.Loose);

    private CreatePurchaseReturnValidator CreateValidator() => new(_mockReasonRepo.Object);

    private void SetupHappyPath()
    {
        _mockReasonRepo
            .Setup(r => r.BelongsToReturnTypeAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupHappyPath();
        var command = PurchaseReturnBuilders.ValidCreateCommand();
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ZeroPoId_FailsValidation()
    {
        SetupHappyPath();
        var command = PurchaseReturnBuilders.ValidCreateCommand(poId: 0);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.PoId);
    }

    [Fact]
    public async Task Validate_ZeroGrnHeaderId_FailsValidation()
    {
        SetupHappyPath();
        var command = PurchaseReturnBuilders.ValidCreateCommand(grnHeaderId: 0);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.GrnHeaderId);
    }

    [Fact]
    public async Task Validate_ZeroReturnTypeId_FailsValidation()
    {
        SetupHappyPath();
        var command = PurchaseReturnBuilders.ValidCreateCommand(returnTypeId: 0);
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ReturnTypeId);
    }

    [Fact]
    public async Task Validate_ReasonNotBelongingToType_FailsValidation()
    {
        _mockReasonRepo
            .Setup(r => r.BelongsToReturnTypeAsync(1, 1))
            .ReturnsAsync(false);

        var command = PurchaseReturnBuilders.ValidCreateCommand();
        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Validate_EmptyDetails_FailsValidation()
    {
        SetupHappyPath();
        var command = PurchaseReturnBuilders.ValidCreateCommand();
        command.Details.Clear();
        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Details);
    }

    [Fact]
    public async Task Validate_DetailReturnQtyExceedsAccepted_FailsValidation()
    {
        SetupHappyPath();
        var command = PurchaseReturnBuilders.ValidCreateCommand();
        command.Details[0].ReturnQty = 100m;
        command.Details[0].AcceptedQty = 50m;

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveAnyValidationError();
    }
}
