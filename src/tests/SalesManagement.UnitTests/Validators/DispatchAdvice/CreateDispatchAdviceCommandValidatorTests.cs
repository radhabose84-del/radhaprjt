using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice;
using SalesManagement.Presentation.Validation.DispatchAdvice;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DispatchAdvice;

public sealed class CreateDispatchAdviceCommandValidatorTests
{
    private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

    private CreateDispatchAdviceCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private static CreateDispatchAdviceCommand ValidCommand() => new()
    {
        DispatchDate = new DateOnly(2026, 1, 1),
        SalesOrderId = 1,
        PartyId = 1,
        DispatchAddressId = 1,
        UnitId = 1
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SalesOrderId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.SalesOrderId = 0;
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
