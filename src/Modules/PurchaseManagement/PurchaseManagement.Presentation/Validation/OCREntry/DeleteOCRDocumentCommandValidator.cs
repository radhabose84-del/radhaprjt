using FluentValidation;
using PurchaseManagement.Application.OCREntry.Commands.DeleteDocument;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.OCREntry;

public class DeleteOCRDocumentCommandValidator : AbstractValidator<DeleteOCRDocumentCommand>
{
    private readonly List<ValidationRule> _validationRules;

    public DeleteOCRDocumentCommandValidator()
    {
        _validationRules = ValidationRuleLoader.LoadValidationRules();
        if (_validationRules == null || _validationRules.Count == 0)
        {
            throw new InvalidOperationException("Validation rules could not be loaded.");
        }

        foreach (var rule in _validationRules)
        {
            switch (rule.Rule)
            {
                case "NotEmpty":
                    RuleFor(x => x.FileName)
                        .NotNull()
                        .WithMessage($"FileName {rule.Error}")
                        .NotEmpty()
                        .WithMessage($"FileName {rule.Error}");
                    break;

                default:
                    break;
            }
        }
    }
}
