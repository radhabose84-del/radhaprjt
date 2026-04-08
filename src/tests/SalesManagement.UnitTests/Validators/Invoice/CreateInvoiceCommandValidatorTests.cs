using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Presentation.Validation.Invoice;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.Invoice;

public sealed class CreateInvoiceCommandValidatorTests
{
    private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

    private CreateInvoiceCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUomLookup.Object);

    private static CreateInvoiceCommand ValidCommand() => new()
    {
        InvoiceDate = new DateOnly(2026, 1, 1),
        DispatchAdviceId = 1,
        PartyId = 1,
        FinancialYearId = 1
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DispatchAdviceId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.DispatchAdviceId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task PartyId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.PartyId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
