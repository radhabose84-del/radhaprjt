using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn;
using SalesManagement.Presentation.Validation.SalesReturn;

namespace SalesManagement.UnitTests.Validators.SalesReturn;

public sealed class CreateSalesReturnCommandValidatorTests
{
    private readonly Mock<ISalesReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private CreateSalesReturnCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private static CreateSalesReturnCommand ValidCommand() => new()
    {
        ReturnDate = new DateOnly(2026, 1, 1),
        ComplaintHeaderId = 1,
        CustomerId = 1,
        WarehouseId = 1,
        BinId = 1
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ComplaintHeaderId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.ComplaintHeaderId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
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
    public async Task WarehouseId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.WarehouseId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
