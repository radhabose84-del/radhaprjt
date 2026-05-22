using Contracts;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Presentation.Validation.SalesOrder;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOrder;

public sealed class CreateSalesOrderCommandValidatorTests
{
    private readonly Mock<ISalesOrderQueryRepository>    _mockQueryRepo      = new(MockBehavior.Loose);
    private readonly Mock<IWorkflowLookup>               _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService>             _mockIpService      = new(MockBehavior.Loose);
    private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter   = new(MockBehavior.Loose);
    private readonly Mock<IAccessPolicyService>          _mockPolicy         = new(MockBehavior.Loose);

    private CreateSalesOrderCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object,
            _mockWorkflowLookup.Object, _mockIpService.Object, _mockAccessFilter.Object,
            _mockPolicy.Object);

    private static CreateSalesOrderCommand ValidCommand() => new()
    {
        SalesOrderDetails = new CreateSalesOrderDto
        {
            SalesOrderTypeId = 1,
            SalesGroupId     = 1,
            PartyId          = 1,
            UnitId           = 1,
            FreightTypeId    = 1,
            EnquiryType      = 1
        }
    };

    [Fact]
    public async Task ValidCommand_RunsWithoutException()
    {
        var result = await CreateValidator().TestValidateAsync(ValidCommand());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task NullDetails_FailsValidation()
    {
        var cmd = new CreateSalesOrderCommand { SalesOrderDetails = null };
        var result = await CreateValidator().TestValidateAsync(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task AccessPolicy_NullAllowed_TypeIdPasses()
    {
        // null = unrestricted — all type IDs allowed
        _mockPolicy
            .Setup(s => s.GetAllowedValueIdsAsync(AccessPolicyCodes.SalesOrderType, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<int>?)null);

        var result = await CreateValidator().TestValidateAsync(ValidCommand());

        result.ShouldNotHaveValidationErrorFor(x => x.SalesOrderDetails!.SalesOrderTypeId);
    }

    [Fact]
    public async Task AccessPolicy_AllowedListContainsTypeId_Passes()
    {
        _mockPolicy
            .Setup(s => s.GetAllowedValueIdsAsync(AccessPolicyCodes.SalesOrderType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 2, 3 }.AsReadOnly());

        var result = await CreateValidator().TestValidateAsync(ValidCommand()); // SalesOrderTypeId = 1

        result.ShouldNotHaveValidationErrorFor(x => x.SalesOrderDetails!.SalesOrderTypeId);
    }

    [Fact]
    public async Task AccessPolicy_AllowedListDoesNotContainTypeId_FailsValidation()
    {
        _mockPolicy
            .Setup(s => s.GetAllowedValueIdsAsync(AccessPolicyCodes.SalesOrderType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 5, 6 }.AsReadOnly()); // typeId 1 is NOT in the list

        var result = await CreateValidator().TestValidateAsync(ValidCommand()); // SalesOrderTypeId = 1

        result.ShouldHaveValidationErrorFor(x => x.SalesOrderDetails!.SalesOrderTypeId)
              .WithErrorMessage("SalesOrderTypeId You are not authorised to use this value.");
    }
}
