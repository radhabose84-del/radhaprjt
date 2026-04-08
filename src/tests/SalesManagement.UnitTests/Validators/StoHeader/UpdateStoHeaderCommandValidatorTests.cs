using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Commands.UpdateStoHeader;
using SalesManagement.Presentation.Validation.StoHeader;

namespace SalesManagement.UnitTests.Validators.StoHeader;

public sealed class UpdateStoHeaderCommandValidatorTests
{
    private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private UpdateStoHeaderCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
    }

    private static UpdateStoHeaderCommand ValidCommand() => new()
    {
        Id = 1,
        DocumentDate = new DateOnly(2026, 1, 1),
        ExpectedDeliveryDate = new DateOnly(2026, 2, 1),
        StoTypeId = 1,
        MovementTypeId = 1,
        SupplyingPlantId = 1,
        SupplyingStorageLocationId = 1,
        ReceivingPlantId = 2,
        ReceivingStorageLocationId = 2,
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
    public async Task StoTypeId_Zero_FailsValidation()
    {
        SetupAllAsyncMocks();
        var cmd = ValidCommand();
        cmd.StoTypeId = 0;
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
