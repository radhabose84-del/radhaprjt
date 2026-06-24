using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CopyJournal;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    public class CopyJournalCommandValidator : AbstractValidator<CopyJournalCommand>
    {
        private readonly IJournalQueryRepository _queryRepository;

        public CopyJournalCommandValidator(IJournalQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                    .WithMessage("Journal voucher not found.")
                .When(x => x.Id > 0);
        }
    }
}
