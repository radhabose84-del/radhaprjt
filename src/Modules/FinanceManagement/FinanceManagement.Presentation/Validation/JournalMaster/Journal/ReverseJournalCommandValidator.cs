using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.ReverseJournal;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    public class ReverseJournalCommandValidator : AbstractValidator<ReverseJournalCommand>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public ReverseJournalCommandValidator(
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                    .WithMessage("Journal voucher not found.")
                .MustAsync(async (id, ct) => await _queryRepository.IsPostedAsync(id))
                    .WithMessage("Only posted journals can be reversed.")
                .MustAsync(async (id, ct) => !await _queryRepository.IsReversedAsync(id))
                    .WithMessage("This journal is already reversed.")
                .MustAsync(async (id, ct) => !await _queryRepository.IsReversalAsync(id))   // AC-4
                    .WithMessage("A reversal journal cannot itself be reversed.");

            // AC-3 — only when an explicit reversal date is supplied (else the handler defaults it to the
            // first day of the next open period).
            When(x => x.ReversalDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .MustAsync(async (x, ct) => await _queryRepository.GetOpenPeriodByDateAsync(CompanyId(), x.ReversalDate!.Value) != null)
                    .WithMessage("Reversal date must fall within an open accounting period.");

                RuleFor(x => x)
                    .MustAsync(async (x, ct) =>
                    {
                        var postingDate = await _queryRepository.GetPostingDateAsync(x.Id);
                        return postingDate == null || x.ReversalDate!.Value >= postingDate.Value;
                    })
                    .WithMessage("Reversal date cannot precede the original posting date.");
            });
        }

        private int CompanyId() => _ipAddressService.GetCompanyId() ?? 0;
    }
}
