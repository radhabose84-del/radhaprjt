using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournalBatch;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    public class PostJournalBatchCommandValidator : AbstractValidator<PostJournalBatchCommand>
    {
        public PostJournalBatchCommandValidator()
        {
            RuleFor(x => x.Ids)
                .NotEmpty().WithMessage("At least one journal id is required.");

            RuleForEach(x => x.Ids)
                .GreaterThan(0).WithMessage("Journal id must be greater than zero.");
        }
    }
}
