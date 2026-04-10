using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution;
using SalesManagement.Presentation.Validation.ComplaintResolution;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ComplaintResolution;

public sealed class UpdateResolutionCommandValidatorTests
{
    private readonly Mock<IComplaintResolutionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private UpdateResolutionCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Updated resolution",
            ClosureRemarks = "Closing remarks",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NotFoundId_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = new UpdateResolutionCommand
        {
            Id = 99,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace",
            ClosureRemarks = "Closing",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_MissingResolutionTypeId_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 0,
            ResolutionSummary = "Replace",
            ClosureRemarks = "Closing",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ResolutionTypeId);
    }

    [Fact]
    public async Task Validate_MissingResolutionSummary_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = null,
            ClosureRemarks = "Closing",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ResolutionSummary);
    }

    [Fact]
    public async Task Validate_MissingClosureRemarks_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace",
            ClosureRemarks = null,
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ClosureRemarks);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
    {
        SetupAllAsyncMocks();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace",
            ClosureRemarks = "Closing",
            IsActive = isActive
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.IsActive);
    }
}
