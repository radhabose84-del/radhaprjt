using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FinanceManagement.Presentation.Validation.JournalMaster.Journal;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.Journal
{
    public sealed class PostJournalCommandValidatorTests
    {
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private PostJournalCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsPostedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsBalancedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsPeriodOpenAsync(It.IsAny<int>())).ReturnsAsync(true);
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
    }
}
