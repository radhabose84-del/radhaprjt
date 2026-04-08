using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Presentation.Validation.ComplaintQCReview;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ComplaintQCReview;

public sealed class UpdateQCReviewCommandValidatorTests
{
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private UpdateQCReviewCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.UserExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateQCReviewCommand
        {
            Id = 1,
            PhysicalVerificationId = 5,
            Comments = "Updated review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
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
        _mockQueryRepo.Setup(r => r.UserExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var command = new UpdateQCReviewCommand
        {
            Id = 99,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_MissingComments_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateQCReviewCommand
        {
            Id = 1,
            PhysicalVerificationId = 5,
            Comments = null,
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Comments);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
    {
        SetupAllAsyncMocks();
        var command = new UpdateQCReviewCommand
        {
            Id = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            IsActive = isActive
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.IsActive);
    }

    [Fact]
    public async Task Validate_LabVerificationRequired_NoLabPerson_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateQCReviewCommand
        {
            Id = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            IsActive = 1,
            LabVerificationRequired = true,
            LabResponsiblePersonId = null
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.LabResponsiblePersonId);
    }
}
