using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    public class PostJournalCommandValidator : AbstractValidator<PostJournalCommand>
    {
        private readonly IJournalQueryRepository _queryRepository;

        public PostJournalCommandValidator(IJournalQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                .WithMessage("Journal voucher not found.")
                .MustAsync(async (id, ct) => !await _queryRepository.IsPostedAsync(id))
                .WithMessage("Journal voucher is already posted.")
                .MustAsync(async (id, ct) => await _queryRepository.IsBalancedAsync(id))
                .WithMessage("Journal voucher must be balanced (total debit = total credit) before posting.")
                .MustAsync(async (id, ct) => await _queryRepository.IsPeriodOpenAsync(id))
                .WithMessage("The journal's accounting period is not open.")
                .When(x => x.Id > 0);
        }
    }
}
