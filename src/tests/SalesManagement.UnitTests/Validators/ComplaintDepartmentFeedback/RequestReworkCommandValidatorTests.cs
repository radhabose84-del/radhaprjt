using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.RequestRework;
using SalesManagement.Presentation.Validation.ComplaintDepartmentFeedback;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ComplaintDepartmentFeedback;

public sealed class RequestReworkCommandValidatorTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private RequestReworkCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int feedbackId = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(feedbackId)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.IsQCApprovedForFeedbackAsync(It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new RequestReworkCommand
        {
            FeedbackId = 1,
            ReworkReason = "Need more detail on root cause analysis"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_MissingFeedbackId_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);

        var command = new RequestReworkCommand
        {
            FeedbackId = 0,
            ReworkReason = "Reason"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.FeedbackId);
    }

    [Fact]
    public async Task Validate_MissingReworkReason_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new RequestReworkCommand
        {
            FeedbackId = 1,
            ReworkReason = null
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ReworkReason);
    }

    [Fact]
    public async Task Validate_FeedbackNotFound_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.IsQCApprovedForFeedbackAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = new RequestReworkCommand
        {
            FeedbackId = 99,
            ReworkReason = "Reason"
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.FeedbackId)
              .WithErrorMessage("Feedback not found.");
    }
}
