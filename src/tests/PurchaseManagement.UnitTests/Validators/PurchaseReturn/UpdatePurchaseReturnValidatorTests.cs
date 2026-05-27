using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Presentation.Validation.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PurchaseReturn;

public sealed class UpdatePurchaseReturnValidatorTests
{
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IReturnReasonQueryRepository> _mockReasonRepo = new(MockBehavior.Loose);

    private UpdatePurchaseReturnValidator CreateValidator() =>
        new(_mockQueryRepo.Object, _mockReasonRepo.Object);

    private void SetupHappyPath()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
        _mockReasonRepo.Setup(r => r.BelongsToReturnTypeAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupHappyPath();
        var result = await CreateValidator().TestValidateAsync(PurchaseReturnBuilders.ValidUpdateCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        _mockReasonRepo.Setup(r => r.BelongsToReturnTypeAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        var result = await CreateValidator().TestValidateAsync(PurchaseReturnBuilders.ValidUpdateCommand(id: 99));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-1)]
    public async Task Validate_IsActiveOutOfRange_FailsValidation(int isActive)
    {
        SetupHappyPath();
        var cmd = PurchaseReturnBuilders.ValidUpdateCommand();
        cmd.IsActive = isActive;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.IsActive);
    }

    [Fact]
    public async Task Validate_EmptyDetails_FailsValidation()
    {
        SetupHappyPath();
        var cmd = PurchaseReturnBuilders.ValidUpdateCommand();
        cmd.Details.Clear();
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Details);
    }
}
