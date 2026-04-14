using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution;
using SalesManagement.Presentation.Validation.ComplaintResolution;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ComplaintResolution;

public sealed class SubmitResolutionCommandValidatorTests
{
    private readonly Mock<IComplaintResolutionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintQueryRepository> _mockComplaintQueryRepo = new(MockBehavior.Loose);

    private SubmitResolutionCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockComplaintQueryRepo.Object);

    private void SetupAllAsyncMocks(int complaintHeaderId = 1)
    {
        _mockQueryRepo.Setup(r => r.ComplaintExistsAsync(complaintHeaderId)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.ResolutionExistsForComplaintAsync(complaintHeaderId, null)).ReturnsAsync(false);
        _mockComplaintQueryRepo.Setup(r => r.IsReadyForResolutionAsync(complaintHeaderId)).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace defective material"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_MissingComplaintHeaderId_FailsValidation()
    {
        // ComplaintHeaderId=0 won't trigger FK/AlreadyExists (.When x > 0)
        // but ResolutionTypeId > 0 triggers MiscMasterExistsAsync
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 0,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ComplaintHeaderId);
    }

    [Fact]
    public async Task Validate_MissingResolutionTypeId_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 0,
            ResolutionSummary = "Replace"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ResolutionTypeId);
    }

    [Fact]
    public async Task Validate_MissingResolutionSummary_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = null
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ResolutionSummary);
    }

    [Fact]
    public async Task Validate_ResolutionAlreadyExists_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.ComplaintExistsAsync(1)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.ResolutionExistsForComplaintAsync(1, null)).ReturnsAsync(true);

        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ComplaintHeaderId)
              .WithErrorMessage("A resolution already exists for this complaint.");
    }

    [Fact]
    public async Task Validate_ComplaintNotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.ComplaintExistsAsync(999)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.ResolutionExistsForComplaintAsync(999, null)).ReturnsAsync(false);

        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 999,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ComplaintHeaderId)
              .WithErrorMessage("Complaint not found.");
    }

    [Fact]
    public async Task Validate_NegativeReturnQuantity_PassesWhenNull()
    {
        SetupAllAsyncMocks();
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace",
            ReturnQuantity = null
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ReturnQuantity);
    }
}
