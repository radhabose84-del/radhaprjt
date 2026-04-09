using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Commands.UpdateInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Presentation.Validation.Invoice;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.Invoice;

public sealed class UpdateInvoiceCommandValidatorTests
{
    private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private UpdateInvoiceCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
    }

    private static UpdateInvoiceCommand ValidCommand() => new()
    {
        Id = 1,
        InvoiceDate = new DateOnly(2026, 1, 1),
        Details = new List<UpdateInvoiceDetailDto> { new() { ItemId = 1, Quantity = 10 } }
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        SetupAllAsyncMocks();
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Id_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.Id = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task NullDetails_FailsValidation()
    {
        SetupAllAsyncMocks();
        var cmd = ValidCommand();
        cmd.Details = null;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
