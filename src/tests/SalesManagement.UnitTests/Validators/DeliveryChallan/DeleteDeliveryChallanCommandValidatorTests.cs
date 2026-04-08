using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan;
using SalesManagement.Presentation.Validation.DeliveryChallan;

namespace SalesManagement.UnitTests.Validators.DeliveryChallan;

public sealed class DeleteDeliveryChallanCommandValidatorTests
{
    private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private DeleteDeliveryChallanCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.HasStoReceiptAsync(id)).ReturnsAsync(false);
    }

    [Fact]
    public async Task ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(new DeleteDeliveryChallanCommand(1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(new DeleteDeliveryChallanCommand(0));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.HasStoReceiptAsync(99)).ReturnsAsync(false);

        var result = await CreateValidator().TestValidateAsync(new DeleteDeliveryChallanCommand(99));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task HasStoReceipt_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.HasStoReceiptAsync(1)).ReturnsAsync(true);

        var result = await CreateValidator().TestValidateAsync(new DeleteDeliveryChallanCommand(1));
        result.ShouldHaveAnyValidationError();
    }
}
