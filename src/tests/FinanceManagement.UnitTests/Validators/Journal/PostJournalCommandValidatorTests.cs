using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FinanceManagement.Presentation.Validation.JournalMaster.Journal;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.Journal
{
    public sealed class PostJournalCommandValidatorTests
    {
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);

        private PostJournalCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockWorkflow.Object, _mockIp.Object, _mockTz.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsPostedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsBalancedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsPeriodOpenAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsPostingEligibleAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockTz.Setup(s => s.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Validate_Postable_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new PostJournalCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new PostJournalCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_AlreadyPosted_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsPostedAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new PostJournalCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_Unbalanced_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsBalancedAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new PostJournalCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ClosedPeriod_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsPeriodOpenAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new PostJournalCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ManualDraft_WithWorkflowConfigured_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsPostingEligibleAsync(It.IsAny<int>())).ReturnsAsync(false);   // manual draft, not approved
            _mockWorkflow.Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);   // approval workflow IS configured → must approve first

            var result = await CreateValidator().TestValidateAsync(new PostJournalCommand(1));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must be approved before posting"));
        }

        [Fact]
        public async Task Validate_ManualDraft_NoWorkflowConfigured_Passes()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsPostingEligibleAsync(It.IsAny<int>())).ReturnsAsync(false);   // manual draft
            _mockWorkflow.Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);   // no approval workflow → may post directly

            var result = await CreateValidator().TestValidateAsync(new PostJournalCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
