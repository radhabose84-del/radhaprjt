using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder;
using SalesManagement.Presentation.Validation.SalesOrder;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOrder;

public sealed class UpdateSalesOrderCommandValidatorTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

    private UpdateSalesOrderCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
    }

    private static UpdateSalesOrderCommand ValidCommand() => new()
    {
        Id = 1,
        SalesGroupId = 1,
        UnitId = 1,
        PartyId = 1,
        FreightTypeId = 1,
        EnquiryType = 1,
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
    public async Task SalesGroupId_Zero_FailsValidation()
    {
        SetupAllAsyncMocks();
        var cmd = ValidCommand();
        cmd.SalesGroupId = 0;
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
