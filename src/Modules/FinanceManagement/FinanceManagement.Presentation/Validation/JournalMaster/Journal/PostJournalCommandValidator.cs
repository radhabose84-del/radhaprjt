using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FinanceManagement.Domain.Common;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    public class PostJournalCommandValidator : AbstractValidator<PostJournalCommand>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

        public PostJournalCommandValidator(
            IJournalQueryRepository queryRepository,
            IWorkflowLookup workflowLookup,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService)
        {
            _queryRepository = queryRepository;
            _workflowLookup = workflowLookup;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                .WithMessage("Journal voucher not found.")
                .MustAsync(async (id, ct) => !await _queryRepository.IsPostedAsync(id))
                .WithMessage("Journal voucher is already posted.")
                .MustAsync(IsEligibleToPostAsync)
                .WithMessage("This manual voucher must be approved before posting (an approval workflow is configured for journals).")
                .MustAsync(async (id, ct) => await _queryRepository.IsBalancedAsync(id))
                .WithMessage("Journal voucher must be balanced (total debit = total credit) before posting.")
                .MustAsync(async (id, ct) => await _queryRepository.IsPeriodOpenAsync(id))
                .WithMessage("The journal's accounting period is not open.")
                .When(x => x.Id > 0);

            // Posted date cannot be in the future (current date is allowed).
            RuleFor(x => x.PostedDate)
                .Must(d => !d.HasValue || d.Value <= Today())
                .WithMessage("Posted date cannot be a future date.");

            // Posted date must be on or after the voucher date.
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var postedDate = cmd.PostedDate ?? Today();
                    var journal = await _queryRepository.GetByIdAsync(cmd.Id);
                    return journal == null || postedDate >= journal.VoucherDate;
                })
                .WithMessage("Posted date must be on or after the voucher date.")
                .When(x => x.Id > 0);

            // A back-dated post (earlier than the current date) requires the voucher to be approved first.
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var postedDate = cmd.PostedDate ?? Today();
                    if (postedDate >= Today())
                        return true;   // current-date posting → no extra approval gate

                    var journal = await _queryRepository.GetByIdAsync(cmd.Id);
                    var approvedStatusId = await _queryRepository.GetStatusIdAsync("APPROVED");
                    return journal != null && journal.StatusId == approvedStatusId;
                })
                .WithMessage("Back-dated posting requires the voucher to be approved first.")
                .When(x => x.Id > 0);
        }

        private DateOnly Today() => DateOnly.FromDateTime(_timeZoneService.GetCurrentTime().DateTime);

        // Postable when APPROVED or a system journal in DRAFT. A manual DRAFT is postable ONLY when no
        // journal approval workflow is configured for the unit (if one exists, it must be approved first).
        private async Task<bool> IsEligibleToPostAsync(int id, CancellationToken ct)
        {
            if (await _queryRepository.IsPostingEligibleAsync(id))
                return true;

            var configured = await _workflowLookup.IsApproveWorkflowConfigureAsync(
                MiscEnumEntity.JournalVoucher, _ipAddressService.GetUnitId() ?? 0, 0);
            return !configured;
        }
    }
}
