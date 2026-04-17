using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback;
using SalesManagement.Presentation.Validation.ComplaintDepartmentFeedback;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ComplaintDepartmentFeedback;

public sealed class UpdateComplaintDepartmentFeedbackCommandValidatorTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private UpdateComplaintDepartmentFeedbackCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.IsQCApprovedForFeedbackAsync(It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateComplaintDepartmentFeedbackCommand
        {
            Id = 1,
            CorrectiveAction = "Updated fix",
            RootCauseText = "Material defect",
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
        _mockQueryRepo.Setup(r => r.IsQCApprovedForFeedbackAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = new UpdateComplaintDepartmentFeedbackCommand
        {
            Id = 99,
            CorrectiveAction = "Fix",
            RootCauseText = "Something",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_MissingCorrectiveAction_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateComplaintDepartmentFeedbackCommand
        {
            Id = 1,
            CorrectiveAction = null,
            RootCauseText = "Something",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.CorrectiveAction);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
    {
        SetupAllAsyncMocks();
        var command = new UpdateComplaintDepartmentFeedbackCommand
        {
            Id = 1,
            CorrectiveAction = "Fix",
            RootCauseText = "Something",
            IsActive = isActive
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.IsActive);
    }
}
