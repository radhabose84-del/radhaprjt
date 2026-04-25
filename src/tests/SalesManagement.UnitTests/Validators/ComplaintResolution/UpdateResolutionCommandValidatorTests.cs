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
        // Default: ClosureStatusId is NOT 'Closed' — passes the new manual-Closed block.
        _mockQueryRepo.Setup(r => r.IsClosureStatusClosedAsync(It.IsAny<int>())).ReturnsAsync(false);
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
            ClosureStatusId = 5,
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

    // ---------------------------------------------------------------------------
    // Block manual ClosureStatus = "Closed"
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Validate_ClosureStatusClosed_FailsValidation()
    {
        const int closedStatusId = 159;  // any id; mock decides the verdict
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.IsClosureStatusClosedAsync(closedStatusId)).ReturnsAsync(true);

        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Resolved",
            ClosureStatusId = closedStatusId,
            ClosureRemarks = "Closing now",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.ClosureStatusId)
              .WithErrorMessage("ClosureStatus 'Closed' cannot be set manually. The system will mark a resolution as Closed only after the downstream action (Credit Note / Sales Return / Replacement) is verified.");
    }

    [Fact]
    public async Task Validate_ClosureStatusOpenOrReadyForClosure_PassesValidation()
    {
        const int readyForClosureId = 158;
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.IsClosureStatusClosedAsync(readyForClosureId)).ReturnsAsync(false);

        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "In progress",
            ClosureStatusId = readyForClosureId,
            ClosureRemarks = "Awaiting verification",
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ClosureStatusId);
    }

    [Fact]
    public async Task Validate_ClosureStatusNull_RuleSkipped()
    {
        // ClosureStatusId is nullable; when null, the ClosureStatus rule chain shouldn't fire.
        SetupAllAsyncMocks();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Resolved",
            ClosureStatusId = null,
            IsActive = 1
        };

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ClosureStatusId);
    }
}
