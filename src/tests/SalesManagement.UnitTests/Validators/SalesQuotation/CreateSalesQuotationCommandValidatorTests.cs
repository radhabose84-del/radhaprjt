using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Presentation.Validation.SalesQuotation;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesQuotation;

public sealed class CreateSalesQuotationCommandValidatorTests
{
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

    private CreateSalesQuotationCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

    private static CreateSalesQuotationCommand ValidCommand() => new()
    {
        CustomerId = 1,
        QuotationDate = new DateOnly(2026, 1, 1),
        ValidityDate = new DateOnly(2026, 2, 1),
        PaymentTermId = 1,
        DeliveryTermId = 1
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.CustomerId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task ValidityDate_BeforeQuotationDate_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.ValidityDate = new DateOnly(2025, 12, 1);
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
