using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.JournalImport
{
    // Shape-level validation only — the row-level data validation lives in the import engine (handler),
    // because it produces a row-level error report rather than a 400 (US-17 AC-1).
    public class ImportJournalsCommandValidator : AbstractValidator<ImportJournalsCommand>
    {
        public ImportJournalsCommandValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.");

            RuleFor(x => x.Rows)
                .NotEmpty().WithMessage("Import file contains no rows.");
        }
    }
}
