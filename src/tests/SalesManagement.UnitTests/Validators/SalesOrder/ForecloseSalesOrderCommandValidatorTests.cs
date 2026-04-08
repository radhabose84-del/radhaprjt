using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.ForecloseSalesOrder;
using SalesManagement.Presentation.Validation.SalesOrder;

namespace SalesManagement.UnitTests.Validators.SalesOrder;

public sealed class ForecloseSalesOrderCommandValidatorTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private ForecloseSalesOrderCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.HasDispatchAdviceAsync(id)).ReturnsAsync(true);
    }

    [Fact]
    public async Task ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(new ForecloseSalesOrderCommand(1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var result = await CreateValidator().TestValidateAsync(new ForecloseSalesOrderCommand(0));
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.HasDispatchAdviceAsync(99)).ReturnsAsync(true);

        var result = await CreateValidator().TestValidateAsync(new ForecloseSalesOrderCommand(99));
        result.ShouldHaveAnyValidationError();
    }
}
