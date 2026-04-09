using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.CreateDeliveryChallan;
using SalesManagement.Presentation.Validation.DeliveryChallan;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DeliveryChallan;

public sealed class CreateDeliveryChallanCommandValidatorTests
{
    private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private CreateDeliveryChallanCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private static CreateDeliveryChallanCommand ValidCommand() => new()
    {
        DeliveryDate = new DateOnly(2026, 1, 1),
        StoHeaderId = 1,
        FromPlantId = 1,
        FromStorageLocationId = 1,
        ToPlantId = 2,
        ToStorageLocationId = 2,
        TransporterId = 1,
        ConsignmentValue = 100m
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task StoHeaderId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.StoHeaderId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task FromPlantId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.FromPlantId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
