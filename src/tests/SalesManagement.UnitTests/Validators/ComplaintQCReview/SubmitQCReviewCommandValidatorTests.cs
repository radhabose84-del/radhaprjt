using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Presentation.Validation.ComplaintQCReview;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ComplaintQCReview;

public sealed class SubmitQCReviewCommandValidatorTests
{
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private SubmitQCReviewCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int complaintHeaderId = 1)
    {
        _mockQueryRepo.Setup(r => r.ComplaintExistsAsync(complaintHeaderId)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.UserExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.ReviewAlreadyExistsAsync(complaintHeaderId, null)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.IsComplaintApprovedAsync(complaintHeaderId)).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review complete",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_MissingComplaintHeaderId_FailsValidation()
    {
        // ComplaintHeaderId=0 won't trigger FK/AlreadyExists (.When x > 0)
        // but PhysicalVerificationId > 0 triggers MiscMasterExistsAsync
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.UserExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 0,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ComplaintHeaderId);
    }

    [Fact]
    public async Task Validate_MissingComments_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = null,
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Comments);
    }

    [Fact]
    public async Task Validate_ReviewAlreadyExists_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.ComplaintExistsAsync(1)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.ReviewAlreadyExistsAsync(1, null)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.IsComplaintApprovedAsync(1)).ReturnsAsync(false);

        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ComplaintHeaderId)
              .WithErrorMessage("A QC Review already exists for this complaint.");
    }

    [Fact]
    public async Task Validate_LabVerificationRequired_NoLabPerson_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            LabVerificationRequired = true,
            LabResponsiblePersonId = null
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.LabResponsiblePersonId)
              .WithErrorMessage("Lab Responsible Person is required when Lab Verification is required.");
    }

    [Fact]
    public async Task Validate_AcceptedWithNoAssignments_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            ComplaintStatusId = 10,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            Assignments = null,
            CompensationStructureId = 4
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Assignments)
              .WithErrorMessage("At least one responsible person must be assigned when complaint is accepted.");
    }
}
