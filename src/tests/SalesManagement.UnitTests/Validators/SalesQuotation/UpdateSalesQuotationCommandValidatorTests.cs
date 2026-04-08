using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.UpdateSalesQuotation;
using SalesManagement.Presentation.Validation.SalesQuotation;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesQuotation;

public sealed class UpdateSalesQuotationCommandValidatorTests
{
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private UpdateSalesQuotationCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
    }

    private static UpdateSalesQuotationCommand ValidCommand() => new()
    {
        Id = 1,
        CustomerId = 1,
        QuotationDate = new DateOnly(2026, 1, 1),
        ValidityDate = new DateOnly(2026, 2, 1),
        PaymentTermId = 1,
        DeliveryTermId = 1,
        IsActive = 1
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerId_Zero_FailsValidation()
    {
        SetupAllAsyncMocks();
        var cmd = ValidCommand();
        cmd.CustomerId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Id_NotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        var cmd = ValidCommand();
        cmd.Id = 99;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
