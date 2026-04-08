using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment;
using SalesManagement.Presentation.Validation.SalesOrderAmendment;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOrderAmendment;

public sealed class CreateSalesOrderAmendmentCommandValidatorTests
{
    private readonly Mock<ISalesOrderAmendmentQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

    private CreateSalesOrderAmendmentCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object,
            _mockWorkflowLookup.Object, _mockIpService.Object);

    private static CreateSalesOrderAmendmentCommand ValidCommand() => new()
    {
        SalesOrderHeaderId = 1,
        Reason = "Price correction"
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SalesOrderHeaderId_Zero_FailsValidation()
    {
        var cmd = ValidCommand();
        cmd.SalesOrderHeaderId = 0;
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
