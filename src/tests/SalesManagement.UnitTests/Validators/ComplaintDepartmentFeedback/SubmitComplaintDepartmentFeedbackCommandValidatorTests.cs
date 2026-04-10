using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.SubmitFeedback;
using SalesManagement.Presentation.Validation.ComplaintDepartmentFeedback;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ComplaintDepartmentFeedback;

public sealed class SubmitComplaintDepartmentFeedbackCommandValidatorTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private SubmitComplaintDepartmentFeedbackCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int assignmentId = 1)
    {
        _mockQueryRepo.Setup(r => r.AssignmentExistsAsync(assignmentId)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.FeedbackAlreadyExistsForAssignmentAsync(assignmentId)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitComplaintDepartmentFeedbackCommand
        {
            AssignmentId = 1,
            CorrectiveAction = "Fix the issue",
            RootCauseText = "Material defect"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_MissingAssignmentId_FailsValidation()
    {
        var command = new SubmitComplaintDepartmentFeedbackCommand
        {
            AssignmentId = 0,
            CorrectiveAction = "Fix"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.AssignmentId);
    }

    [Fact]
    public async Task Validate_MissingCorrectiveAction_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new SubmitComplaintDepartmentFeedbackCommand
        {
            AssignmentId = 1,
            CorrectiveAction = null,
            RootCauseText = "Something"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.CorrectiveAction);
    }

    [Fact]
    public async Task Validate_FeedbackAlreadyExists_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.AssignmentExistsAsync(1)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.FeedbackAlreadyExistsForAssignmentAsync(1)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = new SubmitComplaintDepartmentFeedbackCommand
        {
            AssignmentId = 1,
            CorrectiveAction = "Fix",
            RootCauseText = "Something"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.AssignmentId)
              .WithErrorMessage("Feedback already submitted for this assignment.");
    }

    [Fact]
    public async Task Validate_NoRootCause_FailsBusinessRule()
    {
        SetupAllAsyncMocks();
        var command = new SubmitComplaintDepartmentFeedbackCommand
        {
            AssignmentId = 1,
            CorrectiveAction = "Fix",
            RootCauseText = null,
            RootCauseCategoryId = null
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveAnyValidationError();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("Root Cause is mandatory"));
    }
}
