using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC;
using SalesManagement.Presentation.Validation.DeliveryChallan;

namespace SalesManagement.UnitTests.Validators.DeliveryChallan;

public sealed class GenerateEWaybillForDCCommandValidatorTests
{
    private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private GenerateEWaybillForDCCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    [Fact]
    public async Task ValidId_DCFound_PassesValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

        var result = await CreateValidator().TestValidateAsync(new GenerateEWaybillForDCCommand(1));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(new GenerateEWaybillForDCCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.DeliveryChallanId);
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

        var result = await CreateValidator().TestValidateAsync(new GenerateEWaybillForDCCommand(999));

        result.ShouldHaveValidationErrorFor(x => x.DeliveryChallanId);
    }
}
