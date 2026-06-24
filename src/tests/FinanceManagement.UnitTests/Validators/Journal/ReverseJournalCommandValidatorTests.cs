using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.ReverseJournal;
using FinanceManagement.Presentation.Validation.JournalMaster.Journal;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.Journal
{
    public sealed class ReverseJournalCommandValidatorTests
    {
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);

        private ReverseJournalCommandValidator CreateValidator() => new(_query.Object, _ip.Object);

        private void SetupHappyPath()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            _query.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _query.Setup(r => r.IsPostedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _query.Setup(r => r.IsReversedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _query.Setup(r => r.GetOpenPeriodByDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>())).ReturnsAsync(((int, int)?)(4, 3));
        }

        private static ReverseJournalCommand Cmd() => new() { Id = 7, ReversalDate = new DateOnly(2026, 6, 15) };

        [Fact]
        public async Task Validate_PostedNotReversed_OpenPeriod_Passes()
        {
            SetupHappyPath();
            (await CreateValidator().TestValidateAsync(Cmd())).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            SetupHappyPath();
            _query.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(true);
            (await CreateValidator().TestValidateAsync(Cmd())).ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotPosted_Fails()
        {
            SetupHappyPath();
            _query.Setup(r => r.IsPostedAsync(It.IsAny<int>())).ReturnsAsync(false);
            (await CreateValidator().TestValidateAsync(Cmd())).ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_AlreadyReversed_Fails()
        {
            SetupHappyPath();
            _query.Setup(r => r.IsReversedAsync(It.IsAny<int>())).ReturnsAsync(true);
            (await CreateValidator().TestValidateAsync(Cmd())).ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ClosedPeriod_Fails()
        {
            SetupHappyPath();
            _query.Setup(r => r.GetOpenPeriodByDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>())).ReturnsAsync(((int, int)?)null);

            var result = await CreateValidator().TestValidateAsync(Cmd());

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("open accounting period"));
        }

        [Fact]
        public async Task Validate_ReversalOfAReversal_Fails()   // AC-4
        {
            SetupHappyPath();
            _query.Setup(r => r.IsReversalAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(Cmd());

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot itself be reversed"));
        }

        [Fact]
        public async Task Validate_DateBeforeOriginalPosting_Fails()   // AC-3
        {
            SetupHappyPath();
            _query.Setup(r => r.GetPostingDateAsync(It.IsAny<int>())).ReturnsAsync(new DateOnly(2026, 6, 20));   // posting after reversal date

            var result = await CreateValidator().TestValidateAsync(Cmd());   // ReversalDate = 2026-06-15

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("precede the original posting date"));
        }

        [Fact]
        public async Task Validate_NoDate_SkipsDateRules_Passes()   // AC-3 default path → handler resolves date
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new ReverseJournalCommand { Id = 7, ReversalDate = null });

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
