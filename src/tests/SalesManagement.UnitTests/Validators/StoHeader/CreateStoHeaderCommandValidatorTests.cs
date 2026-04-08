using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Commands.CreateStoHeader;
using SalesManagement.Presentation.Validation.StoHeader;

namespace SalesManagement.UnitTests.Validators.StoHeader;

public sealed class CreateStoHeaderCommandValidatorTests
{
    private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private CreateStoHeaderCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private static CreateStoHeaderCommand ValidCommand() => new()
    {
        DocumentDate = new DateOnly(2026, 1, 1),
        ExpectedDeliveryDate = new DateOnly(2026, 2, 1),
        StoTypeId = 1,
        MovementTypeId = 1,
        SupplyingPlantId = 1,
        SupplyingStorageLocationId = 1,
        ReceivingPlantId = 2,
        ReceivingStorageLocationId = 2
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task StoTypeId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.StoTypeId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task SupplyingPlantId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.SupplyingPlantId = 0;
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
