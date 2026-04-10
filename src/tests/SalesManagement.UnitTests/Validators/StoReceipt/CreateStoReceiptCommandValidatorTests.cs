using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt;
using SalesManagement.Presentation.Validation.StoReceipt;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.StoReceipt;

public sealed class CreateStoReceiptCommandValidatorTests
{
    private readonly Mock<IStoReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private CreateStoReceiptCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private static CreateStoReceiptCommand ValidCommand() => new()
    {
        StoReceiptDate = new DateOnly(2026, 1, 1),
        DeliveryChallanHeaderId = 1,
        ReceivingPlantId = 1,
        ReceivingStorageLocationId = 1
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeliveryChallanHeaderId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.DeliveryChallanHeaderId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task ReceivingPlantId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.ReceivingPlantId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
